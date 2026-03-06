using System;
using System.Collections.Generic;

namespace PoeFixer;

public class RemoveAllPet : IPatch
{
    public string[] FilesToPatch => ["metadata/pet/pet.otc"];
    public string[] DirectoriesToPatch => [];
    public string Extension => "*.otc";

    public string? PatchFile(string text)
    {
        // Nếu file đã có đoạn code này rồi, trả về null
        if (text.Contains("on_construction_complete = \"Delete();\"")) return null;

        string anchor = "extends \"nothing\"";
        if (text.Contains(anchor))
        {
            return text.Replace(anchor, anchor + "\n\nBaseEvents\n{\n\ton_construction_complete = \"Delete();\"\n}");
        }
        return null;
    }

    public bool ShouldPatch(Dictionary<string, bool> bools, Dictionary<string, float> floats)
    {
        return bools.TryGetValue("removeAllPet", out bool enabled) && enabled;
    }
}