using System;
using System.Collections.Generic;

namespace PoeFixer;

public class NamelessSeer : IPatch
{

    public string Extension => "*.otc";

    public string[] FilesToPatch => [
        // Sử dụng đường dẫn chuẩn hóa (thường là Metadata/Npc/...)
        // Bạn nên kiểm tra lại bằng VisualGGPK xem là "Npc" hay "NPC"
        "metadata/npc/league/azmeri/uniquedealermaps.otc"
    ];

    public string[] DirectoriesToPatch => [];

    public bool ShouldPatch(Dictionary<string, bool> bools, Dictionary<string, float> floats)
    {
        bools.TryGetValue("NamelessSeerEnable", out bool enabled);
        return enabled;
    }

    public string? PatchFile(string text)
    {
        // 1. Kiểm tra xem file này có đúng là Nameless Seer (UniqueDealer) không
        if (text.Contains("StateMachine") && text.Contains("on_or_create_state_seen_1"))
        {
            // Kiểm tra xem đã patch chưa để tránh chèn đè nhiều lần
            if (text.Contains("ProximityTrigger"))
            {
                return text; // Đã có rồi thì không sửa nữa
            }

            // 2. Thêm ProximityTrigger vào cuối file
            // Chúng ta dùng toán tử cộng chuỗi để giữ nguyên nội dung gốc và nối thêm đoạn mới
            string patch = "\r\n\r\n// --- Added by NamelessSeer Patch ---\r\n" + @"ProximityTrigger
{
	radius = 150
	condition = ""players""
	on_triggered = ""PlayTextAudio( NavaliOnCrafting , Metadata/NPC/League/Prophecy/NavaliWild, 0);""
}";
            return text + patch;
        }

        return text;
    }
}