using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PoeFixer;

public class RemoveItem : IPatch
{
    // Danh sách các thư mục mục tiêu theo yêu cầu của bạn
    public string[] FilesToPatch => [];

    public string[] DirectoriesToPatch => [
        "metadata/items/armours",
        "metadata/items/quivers",
        "metadata/items/weapons"
    ];

    // Chỉ quét các file định dạng .aoc
    public string Extension => "*.aoc";

    public string? PatchFile(string text)
    {
        bool modified = false;
        string result = text;

        // 1. Định nghĩa Pattern để tìm khối SkinMesh và ObjectHiding
        // Regex này sẽ bắt trọn từ tên khối cho đến dấu đóng ngoặc }
        string blockPattern = @"(SkinMesh|ObjectHiding)\s*\{[^{}]*\}";

        // 2. Kiểm tra và thực hiện xóa khối
        if (Regex.IsMatch(result, blockPattern))
        {
            // Xóa các khối dữ liệu
            result = Regex.Replace(result, blockPattern, string.Empty);
            modified = true;
        }

        // 3. XỬ LÝ "XÓA SẠCH DẤU VẾT": Loại bỏ các dòng trống dư thừa
        // Regex này tìm các khoảng trắng/dòng trống liên tiếp (từ 3 dòng trở lên) 
        // và thu gọn chúng lại để tránh để lại khoảng hở lớn trong file.
        if (modified)
        {
            // Thay thế nhiều dòng trống liên tiếp bằng tối đa 2 dòng xuống dòng
            result = Regex.Replace(result, @"(\r\n|\n){3,}", "\n\n");

            // Cắt bỏ khoảng trắng thừa ở đầu và cuối file
            result = result.Trim();
        }

        // Trả về kết quả nếu có thay đổi, ngược lại trả về null để báo file đã chuẩn
        return modified ? result : null;
    }

    public bool ShouldPatch(Dictionary<string, bool> bools, Dictionary<string, float> floats)
    {
        // Kích hoạt khi người dùng tích vào checkbox "removeItem" trên giao diện
        return bools.TryGetValue("removeItem", out bool enabled) && enabled;
    }
}