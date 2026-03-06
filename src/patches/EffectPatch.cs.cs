using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PoeFixer;

public class EffectPatch : IPatch
{
    // Chỉ định thư mục chứa các hiệu ứng kỹ năng (Spells)
    public string[] DirectoriesToPatch => ["metadata/effects/spells/"];

    public string[] FilesToPatch => [];

    // Chỉ tác động vào file .aoc (Action Object Controller)
    public string Extension => "*.aoc";

    // Danh sách các khối chức năng sẽ bị xóa bỏ hoàn toàn khỏi file
    private readonly string[] _functionsToRemove = {
        "ParticleEffects",
        "TrailsEffects",
        "DecalEvents",
        "ScreenShake",
        "Lights"
    };

    public string? PatchFile(string text)
    {
        string modifiedText = text;

        foreach (var func in _functionsToRemove)
        {
            modifiedText = RemoveFunctionBlock(modifiedText, func);
        }

        // Nếu không có gì thay đổi so với gốc thì trả về null để tránh ghi đè file vô ích
        return modifiedText == text ? null : modifiedText;
    }

    /// <summary>
    /// Thuật toán tìm và xóa toàn bộ khối mã dựa trên đếm dấu đóng mở ngoặc { }
    /// </summary>
    private string RemoveFunctionBlock(string data, string functionName)
    {
        int index = 0;
        while (index < data.Length)
        {
            // Tìm vị trí tên hàm (vd: ParticleEffects)
            int funcIndex = data.IndexOf(functionName, index);
            if (funcIndex < 0) break;

            // Tìm dấu mở ngoặc đầu tiên sau tên hàm
            int openBraceIndex = data.IndexOf('{', funcIndex + functionName.Length);
            if (openBraceIndex < 0)
            {
                index = funcIndex + functionName.Length;
                continue;
            }

            int braceCount = 1;
            int i = openBraceIndex + 1;

            // Duyệt để tìm dấu đóng ngoặc tương ứng (xử lý được cả các ngoặc lồng nhau)
            while (i < data.Length && braceCount > 0)
            {
                if (data[i] == '{') braceCount++;
                else if (data[i] == '}') braceCount--;
                i++;
            }

            if (braceCount == 0)
            {
                // Xóa toàn bộ đoạn từ tên hàm đến dấu đóng ngoặc cuối cùng
                data = data.Remove(funcIndex, i - funcIndex);
                index = funcIndex; // Tiếp tục tìm kiếm từ vị trí vừa xóa
            }
            else
            {
                break;
            }
        }
        return data;
    }

    public bool ShouldPatch(Dictionary<string, bool> bools, Dictionary<string, float> floats)
    {
        // Kích hoạt khi CheckBox "Replace Skill Effects (JSON)" được tích 
        // (Hoặc bạn có thể đổi tên x:Name trong XAML thành "removeEffects")
        return bools.TryGetValue("removeEffect", out bool enabled) && enabled;
    }
}