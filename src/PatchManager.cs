using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PoeFixer;

public class PatchManager
{
    public HashSet<string> patchedFiles = [];
    public HashSet<string> SkipPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public LibBundle3.Index index;
    public string CachePath { get; set; }
    public string ModifiedCachePath { get; set; }

    public Dictionary<string, bool> bools = [];
    public Dictionary<string, float> floats = [];

    public MainWindow window;

    public PatchManager(LibBundle3.Index index, MainWindow window)
    {
        CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extractedassets/");
        ModifiedCachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modifiedassets/");

        this.index = index;
        this.window = window;

        // Load danh sách skip khi khởi tạo
        LoadSkipConfig();
    }

    private void LoadSkipConfig()
    {
        string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
        if (File.Exists(configPath))
        {
            var lines = File.ReadAllLines(configPath);
            bool isSkipSection = false;
            foreach (var line in lines)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                if (trimmed.Equals("[Skip]", StringComparison.OrdinalIgnoreCase))
                {
                    isSkipSection = true;
                    continue;
                }

                if (isSkipSection && !trimmed.StartsWith("["))
                {
                    // XÓA BỎ Replace('\\', '/') - Giữ nguyên định dạng từ file config.ini
                    SkipPaths.Add(trimmed);
                }
            }
        }
    }

    private bool ShouldSkip(string path)
    {
        // Chuyển đường dẫn file từ ổ cứng sang dạng tương đối (loại bỏ thư mục gốc)
        // XÓA BỎ Replace('\\', '/') tại đây
        string gamePath = path.Replace(CachePath, "").Replace(ModifiedCachePath, "");

        if (gamePath.StartsWith("\\") || gamePath.StartsWith("/"))
            gamePath = gamePath.Substring(1);

        // Kiểm tra xem file có thuộc danh sách skip không
        return SkipPaths.Any(skip => gamePath.StartsWith(skip, StringComparison.OrdinalIgnoreCase));
    }

    public int RestoreExtractedAssets()
    {
        if (File.Exists("patch.zip")) File.Delete("patch.zip");
        ZipFile.CreateFromDirectory(CachePath, "patch.zip");
        ZipArchive archive = ZipFile.OpenRead("patch.zip");
        int count = LibBundle3.Index.Replace(index, archive.Entries);
        archive.Dispose();
        File.Delete("patch.zip");
        return count;
    }

    public (int patched, List<string> skippedList) ModifyDirectoryWithDetails(string path, string extension, IPatch patch)
    {
        string fullPath = Path.Combine(CachePath, path);
        List<string> skippedList = new List<string>();
        int patchedCount = 0;

        if (!Directory.Exists(fullPath)) return (0, skippedList);

        IEnumerable<string> files = Directory.EnumerateFiles(fullPath, extension, SearchOption.AllDirectories);

        foreach (string file in files)
        {
            if (ShouldSkip(file))
            {
                // Lấy đường dẫn tương đối để log cho gọn
                string relativePath = file.Replace(CachePath, "").Replace(ModifiedCachePath, "");
                skippedList.Add(relativePath);
                continue;
            }
            if (ModifyFile(file, patch)) patchedCount++;
        }
        return (patchedCount, skippedList);
    }

    public void CollectSettings()
    {
        bools.Clear();
        floats.Clear();
        FindControlsRecursive(window);
    }

    private void FindControlsRecursive(DependencyObject parent)
    {
        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is CheckBox checkbox && !string.IsNullOrEmpty(checkbox.Name))
                bools[checkbox.Name] = checkbox.IsChecked == true;
            else if (child is Slider slider && !string.IsNullOrEmpty(slider.Name))
                floats[slider.Name] = (float)slider.Value;

            FindControlsRecursive(child);
        }
    }

    public (List<string> patchedList, List<string> skippedList) ModifyDirectoryWithFullDetails(string path, string extension, IPatch patch)
    {
        string fullPath = Path.Combine(CachePath, path);
        List<string> patchedList = new List<string>();
        List<string> skippedList = new List<string>();

        if (!Directory.Exists(fullPath)) return (patchedList, skippedList);

        IEnumerable<string> files = Directory.EnumerateFiles(fullPath, extension, SearchOption.AllDirectories);

        foreach (string file in files)
        {
            // Lấy đường dẫn tương đối để log cho đẹp
            string relativePath = file.Replace(CachePath, "").Replace(ModifiedCachePath, "");
            if (relativePath.StartsWith("\\") || relativePath.StartsWith("/"))
                relativePath = relativePath.Substring(1);

            if (ShouldSkip(file))
            {
                skippedList.Add(relativePath);
                continue;
            }

            if (ModifyFile(file, patch))
            {
                patchedList.Add(relativePath);
            }
        }
        return (patchedList, skippedList);
    }

    public int Patch()
    {
        Type[] patchTypes = Assembly.GetExecutingAssembly().GetTypes()
        .Where(x => x.GetInterfaces().Contains(typeof(IPatch))).ToArray();

        IPatch[] patches = new IPatch[patchTypes.Length];
        for (int i = 0; i < patchTypes.Length; i++)
            patches[i] = (IPatch)Activator.CreateInstance(patchTypes[i])!;

        patches = patches.Where(x => x.ShouldPatch(bools, floats)).ToArray();

        if (Directory.Exists(ModifiedCachePath))
            Directory.Delete(ModifiedCachePath, true);

        Directory.CreateDirectory(ModifiedCachePath);

        foreach (IPatch patch in patches)
        {
            Stopwatch stopWatch = new();
            stopWatch.Start();

            // Reset danh sách cho mỗi bản Patch
            List<string> skippedFilesList = new List<string>();
            List<string> patchedFilesList = new List<string>();

            // 1. Xử lý các file đơn lẻ (FilesToPatch)
            foreach (string file in patch.FilesToPatch)
            {
                string fullPath = Path.Combine(CachePath, file);
                if (ShouldSkip(fullPath))
                {
                    skippedFilesList.Add(file);
                    continue;
                }

                // Dùng TryModifyFile và kiểm tra kết quả trả về
                if (TryModifyFile(file, patch))
                {
                    patchedFilesList.Add(file);
                }
            }

            // 2. Xử lý các thư mục (DirectoriesToPatch)
            foreach (string directory in patch.DirectoriesToPatch)
            {
                string[] extensions = patch.Extension.Split('|');
                foreach (var ext in extensions)
                {
                    var result = ModifyDirectoryWithFullDetails(directory, ext, patch);
                    patchedFilesList.AddRange(result.patchedList);
                    skippedFilesList.AddRange(result.skippedList);
                }
            }

            stopWatch.Stop();

            // SỬA LỖI TẠI ĐÂY: Kiểm tra patchedFilesList.Count thay vì biến patchedCount
            if (patchedFilesList.Count > 0)
            {
                var successColor = new SolidColorBrush(Color.FromRgb(16, 185, 129));
              
                    window.EmitToConsole($"[Thành công] {patch.GetType().Name}: Đã sửa {patchedFilesList.Count} file.", successColor);

                    // In chi tiết từng file đã sửa
                    foreach (var pFile in patchedFilesList)
                    {
                        window.EmitToConsole($"      > {pFile}", successColor);
                    }
            }
            else
            {
                // Nếu không có file nào được sửa VÀ cũng không có file nào bị skip thì mới báo lỗi thực sự
                var failColor = new SolidColorBrush(Color.FromRgb(239, 68, 68));
               
                    window.EmitToConsole($"[Thất bại] {patch.GetType().Name}: Không tìm thấy file hoặc không có gì để sửa.", failColor);
            }

            // Log các file bị Skip (giữ nguyên logic của bạn)
            if (skippedFilesList.Count > 0)
            {
                var skipColor = new SolidColorBrush(Color.FromRgb(251, 191, 36));
               
                    window.EmitToConsole($"   -> Đã bỏ qua {skippedFilesList.Count} file từ config.ini:", skipColor);
                foreach (var skipFile in skippedFilesList)
                {
                    window.EmitToConsole($"      + {skipFile}", skipColor);
                }
            }
        }

        if (File.Exists("patch.zip")) File.Delete("patch.zip");
        ZipFile.CreateFromDirectory(ModifiedCachePath, "patch.zip");
        ZipArchive archive = ZipFile.OpenRead("patch.zip");
        int count = LibBundle3.Index.Replace(index, archive.Entries);
        archive.Dispose();

        return count;
    }

    public (int patched, int skipped) ModifyDirectory(string path, string extension, IPatch patch)
    {
        string fullPath = Path.Combine(CachePath, path);
        if (!Directory.Exists(fullPath)) return (0, 0);

        int patchedCount = 0;
        int skippedCount = 0;
        IEnumerable<string> files = Directory.EnumerateFiles(fullPath, extension, SearchOption.AllDirectories);

        foreach (string file in files)
        {
            if (ShouldSkip(file))
            {
                skippedCount++;
                continue;
            }
            if (ModifyFile(file, patch)) patchedCount++;
        }
        return (patchedCount, skippedCount);
    }

    public bool TryModifyFile(string path, IPatch patch)
    {
        string fullPath = Path.Combine(CachePath, path);
        if (File.Exists(fullPath)) return ModifyFile(fullPath, patch);
        return false;
    }

    public bool ModifyFile(string path, IPatch patch)
    {
        // 1. Xác định file này đã từng được sửa chưa
        bool patchModifiedAsset = patchedFiles.Contains(path);
        string currentPath = patchModifiedAsset ? path.Replace(CachePath, ModifiedCachePath) : path;

        if (!File.Exists(currentPath)) return false;

        // 2. Đọc nội dung hiện tại
        string text = File.ReadAllText(currentPath);
        string? modifiedText = patch.PatchFile(text);

        // 3. LOGIC QUAN TRỌNG:
        // Nếu nội dung không thay đổi (do đã sửa từ trước hoặc Patch không tìm thấy chuỗi cần thay)
        if (modifiedText == null || modifiedText == text)
        {
            // Nếu file này ĐÃ nằm trong thư mục modifiedassets, nghĩa là nó ĐÃ được sửa thành công.
            // Chúng ta trả về true để Log hiện màu xanh [Thành công].
            return patchModifiedAsset;
        }

        // 4. Nếu có sự thay đổi mới, thực hiện ghi file
        string savePath = path.Replace(CachePath, ModifiedCachePath);
        Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

        Encoding encoding = Path.GetExtension(savePath).Equals(".hlsl", StringComparison.OrdinalIgnoreCase)
                            ? Encoding.ASCII : Encoding.Unicode;

        File.WriteAllText(savePath, modifiedText, encoding);

        // Đánh dấu file đã được sửa
        if (!patchModifiedAsset) patchedFiles.Add(path);

        return true;
    }
}