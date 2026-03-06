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
                    SkipPaths.Add(trimmed);
                }
            }
        }
    }

    private bool ShouldSkip(string path)
    {
        string gamePath = path.Replace(CachePath, "").Replace(ModifiedCachePath, "");

        if (gamePath.StartsWith("\\") || gamePath.StartsWith("/"))
            gamePath = gamePath.Substring(1);

        return SkipPaths.Any(skip => gamePath.StartsWith(skip, StringComparison.OrdinalIgnoreCase));
    }

    public void CollectSettings()
    {
        bools.Clear();
        floats.Clear();

        // Quét Window chính
        FindControlsRecursive(window);

        // Ép quét thêm bên trong Popup vì nó nằm trên lớp Layer khác
        if (window.CustomSettingsPopup != null && window.CustomSettingsPopup.Child != null)
        {
            FindControlsRecursive(window.CustomSettingsPopup.Child);
        }
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
        // 1. Cập nhật dữ liệu từ UI trước khi chạy
        CollectSettings();

        // 2. Tìm và khởi tạo các bản Patch
        Type[] patchTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(x => x.GetInterfaces().Contains(typeof(IPatch))).ToArray();

        IPatch[] patches = new IPatch[patchTypes.Length];
        for (int i = 0; i < patchTypes.Length; i++)
            patches[i] = (IPatch)Activator.CreateInstance(patchTypes[i])!;

        // 3. Lọc các bản Patch dựa trên UI
        patches = patches.Where(x => x.ShouldPatch(bools, floats)).ToArray();

        // 4. Dọn dẹp thư mục tạm
        if (Directory.Exists(ModifiedCachePath))
            Directory.Delete(ModifiedCachePath, true);

        Directory.CreateDirectory(ModifiedCachePath);

        // 5. Thực hiện Patch từng phần
        foreach (IPatch patch in patches)
        {
            List<string> skippedFilesList = new List<string>();
            List<string> patchedFilesList = new List<string>();

            // Xử lý File đơn lẻ
            foreach (string file in patch.FilesToPatch)
            {
                string fullPath = Path.Combine(CachePath, file);
                if (ShouldSkip(fullPath))
                {
                    skippedFilesList.Add(file);
                    continue;
                }

                if (TryModifyFile(file, patch))
                    patchedFilesList.Add(file);
            }

            // Xử lý Thư mục
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

            // Log kết quả ra Console
            if (patchedFilesList.Count > 0)
            {
                var successColor = new SolidColorBrush(Color.FromRgb(16, 185, 129));
                window.EmitToConsole($"[Thành công] {patch.GetType().Name}: Đã sửa {patchedFilesList.Count} file.", successColor);
                foreach (var pFile in patchedFilesList)
                    window.EmitToConsole($"      > {pFile}", successColor);
            }
            else
            {
                var failColor = new SolidColorBrush(Color.FromRgb(239, 68, 68));
                window.EmitToConsole($"[Thất bại] {patch.GetType().Name}: Không tìm thấy file hoặc không có gì để sửa.", failColor);
            }

            if (skippedFilesList.Count > 0)
            {
                var skipColor = new SolidColorBrush(Color.FromRgb(251, 191, 36));
                window.EmitToConsole($"   -> Đã bỏ qua {skippedFilesList.Count} file từ config.ini:", skipColor);
                foreach (var skipFile in skippedFilesList)
                    window.EmitToConsole($"      + {skipFile}", skipColor);
            }
        }

        // 6. Đóng gói và cập nhật vào GGPK
        int finalCount = 0;
        if (Directory.Exists(ModifiedCachePath) && Directory.EnumerateFiles(ModifiedCachePath, "*", SearchOption.AllDirectories).Any())
        {
            if (File.Exists("patch.zip")) File.Delete("patch.zip");
            ZipFile.CreateFromDirectory(ModifiedCachePath, "patch.zip");

            using (ZipArchive archive = ZipFile.OpenRead("patch.zip"))
            {
                LibBundle3.Index.Replace(index, archive.Entries);
            }

            // Đếm thực tế số file trong thư mục đã modify để báo log cho chuẩn
            finalCount = Directory.GetFiles(ModifiedCachePath, "*", SearchOption.AllDirectories).Length;

            if (File.Exists("patch.zip")) File.Delete("patch.zip");
        }

        return finalCount;
    }

    public bool TryModifyFile(string path, IPatch patch)
    {
        string fullPath = Path.Combine(CachePath, path);
        if (File.Exists(fullPath)) return ModifyFile(fullPath, patch);
        return false;
    }

    public bool ModifyFile(string path, IPatch patch)
    {
        bool patchModifiedAsset = patchedFiles.Contains(path);
        string currentPath = patchModifiedAsset ? path.Replace(CachePath, ModifiedCachePath) : path;

        if (!File.Exists(currentPath)) return false;

        string text = File.ReadAllText(currentPath);
        string? modifiedText = patch.PatchFile(text);

        if (modifiedText == null || modifiedText == text)
        {
            return patchModifiedAsset;
        }

        string savePath = path.Replace(CachePath, ModifiedCachePath);
        Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

        Encoding encoding = Path.GetExtension(savePath).Equals(".hlsl", StringComparison.OrdinalIgnoreCase)
                            ? Encoding.ASCII : Encoding.Unicode;

        File.WriteAllText(savePath, modifiedText, encoding);

        if (!patchModifiedAsset) patchedFiles.Add(path);

        return true;
    }
    public int RestoreExtractedAssets()
    {
        // 1. Kiểm tra xem thư mục tài nguyên gốc có tồn tại không
        if (!Directory.Exists(CachePath)) return 0;

        // 2. Tạo file zip từ thư mục gốc (Vanilla)
        if (File.Exists("patch.zip")) File.Delete("patch.zip");
        ZipFile.CreateFromDirectory(CachePath, "patch.zip");

        int count = 0;
        using (ZipArchive archive = ZipFile.OpenRead("patch.zip"))
        {
            // 3. Ghi đè ngược lại vào GGPK/BIN để khôi phục trạng thái ban đầu
            count = LibBundle3.Index.Replace(index, archive.Entries);
        }

        // 4. Dọn dẹp
        if (File.Exists("patch.zip")) File.Delete("patch.zip");

        // Xóa sạch thư mục đã chỉnh sửa để tránh nhầm lẫn cho lần patch sau
        if (Directory.Exists(ModifiedCachePath))
            Directory.Delete(ModifiedCachePath, true);

        patchedFiles.Clear();

        return count;
    }
}