using System;
using System.Collections.Generic;
using System.Linq;

namespace PoeFixer;

public class RealMap : IPatch
{
    // Nhắm mục tiêu vào thư mục shaders
    public string[] FilesToPatch => [];

    public string[] DirectoriesToPatch => ["shaders/"];

    public string Extension => "minimap_visibility_pixel.hlsl|minimap_blending_pixel.hlsl";

    public string? PatchFile(string text)
    {
        // 1. Xử lý file minimap_visibility_pixel.hlsl
        // Mục tiêu: Làm bản đồ luôn hiển thị (Reveal)
        if (text.Contains("res_color = float4(1.0f, 0.0f, 0.0f, 1.0f);"))
        {
            if (text.Contains("res_color = max(res_color, 0.18f);"))
            {
                return null; // Đã patch rồi
            }

            string anchor = "res_color = float4(1.0f, 0.0f, 0.0f, 1.0f);";
            string insertText = "\n\tres_color = max(res_color, 0.18f);";
            return text.Replace(anchor, anchor + insertText);
        }

        // 2. Xử lý file minimap_blending_pixel.hlsl
        // Mục tiêu: Chỉnh sửa màu sắc và độ rõ nét của đường đi trên minimap
        if (text.Contains("float4 walkable_color"))
        {
            string modifiedText = text;

            // Thay đổi màu sắc vùng có thể đi bộ
            modifiedText = modifiedText.Replace(
                "float4 walkable_color = float4(1.0f, 1.0f, 1.0f, 0.01f);",
                "float4 walkable_color = float4(0.0f, 0.0f, 0.0f, 0.3f);"
            );

            // Thay đổi màu sắc cạnh/biên của bản đồ
            modifiedText = modifiedText.Replace(
                "float4 walkability_map_color = lerp(walkable_color, float4(0.5f, 0.5f, 1.0f, 0.5f), walkable_to_edge_ratio);",
                "float4 walkability_map_color = lerp(walkable_color, float4(12.0f, 12.0f, 12.0f, 0.1f), walkable_to_edge_ratio);"
            );

            return modifiedText == text ? null : modifiedText;
        }

        return null;
    }

    public bool ShouldPatch(Dictionary<string, bool> bools, Dictionary<string, float> floats)
    {
        // Kích hoạt khi tích vào ô "Reveal Map" trên giao diện của bạn
        return bools.TryGetValue("revealMap", out bool enabled) && enabled;
    }
}