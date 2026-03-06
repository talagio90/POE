using LibBundle3.Nodes;
using Newtonsoft.Json;
using System;
using System.IO;

namespace PoeFixer;

public class FileExtractor
{
    public LibBundle3.Index index;
    public const string extractJsonPath = "paths_to_extract.json";

    public FileExtractor(LibBundle3.Index index)
    {
        this.index = index;
    }

    // Nhận thêm một Action để log
    public int ExtractFiles(Action<string> logCallback)
    {
        string cachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extractedassets/");

        // Dọn dẹp thư mục cũ
        if (Directory.Exists(cachePath)) Directory.Delete(cachePath, true);

        if (!File.Exists(extractJsonPath)) return 0;

        string jsonContent = File.ReadAllText(extractJsonPath);
        var pathData = Newtonsoft.Json.JsonConvert.DeserializeObject<PathData>(jsonContent);

        if (pathData == null || pathData.paths == null) return 0;

        int count = 0;
        int nextLogThreshold = 1000; // Biến theo dõi ngưỡng 1000 file tiếp theo
        string? lastDirectory = null; // Lưu vết thư mục vừa log

        foreach (string path in pathData.paths)
        {
            if (index.TryFindNode(path, out var node) && node != null)
            {
                string directory = Path.GetDirectoryName(path) ?? "Root";

                // CHỈ LOG KHI VÀO THƯ MỤC MỚI
                if (directory != lastDirectory)
                {
                    logCallback?.Invoke($"Processing Directory: {directory}...");
                    lastDirectory = directory;
                }

                // Thực hiện trích xuất
                int extractedInThisNode = LibBundle3.Index.ExtractParallel(node, Path.Combine(cachePath, directory));
                count += extractedInThisNode;

                // Log tiến trình khi vượt qua ngưỡng 1000 file (Sửa lỗi mất log của phép %)
                if (count >= nextLogThreshold)
                {
                    logCallback?.Invoke($"--- Progress: {count} files extracted ---");

                    // Cập nhật ngưỡng tiếp theo (ví dụ: đang 1500 thì ngưỡng mới là 2000)
                    nextLogThreshold = ((count / 1000) + 1) * 1000;
                }
            }
        }
        return count;
    }
}