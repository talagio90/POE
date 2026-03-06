using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PoeFixer;

public class Colour : IPatch
{
    public static Dictionary<string, (string Color, bool IsEnabled)> ConfigSnapshot { get; set; } = new();
    public string[] FilesToPatch => [];
    public string[] DirectoriesToPatch => ["metadata/statdescriptions/"];
    public string Extension => "*.txt";

    // Bảng màu chuẩn của PoE
    private readonly Dictionary<string, string> _colorTags = new()
    {
        { "red", "premiumchatoutline" },
        { "green", "quest" },
        { "blue", "divination" },
        { "yellow", "necropolisupside" },
        { "pink", "archnemesismodchaospurple" },
        { "white", "bestiarymod" },
        { "purple", "bloodlinemod" },
    };

    private enum ReadState { ReadingToDescription, ReadingDescription, ReadingData, WritingData }

    public string? PatchFile(string text)
    {
        var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
        ReadState state = ReadState.ReadingToDescription;
        string? currentTag = null;
        int linesToWrite = 0;
        bool modified = false;

        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            string trimmed = line.Trim();

            if (trimmed.StartsWith("description"))
            {
                state = ReadState.ReadingDescription;
                continue;
            }

            if (state == ReadState.ReadingDescription)
            {
                string[] parts = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    string modKey = parts[1];
                    // Kiểm tra xem Mod này có được tích ở UI không
                    if (ConfigSnapshot.TryGetValue(modKey, out var config) && config.IsEnabled)
                    {
                        if (_colorTags.TryGetValue(config.Color.ToLower(), out var tag))
                        {
                            currentTag = tag;
                            state = ReadState.ReadingData;
                            continue;
                        }
                    }
                }
                state = ReadState.ReadingToDescription;
            }
            else if (state == ReadState.ReadingData)
            {
                if (string.IsNullOrWhiteSpace(trimmed)) continue;
                string firstNumber = trimmed.Split(' ')[0];
                if (int.TryParse(firstNumber, out int value))
                {
                    linesToWrite = value;
                    state = ReadState.WritingData;
                }
            }
            else if (state == ReadState.WritingData)
            {
                if (line.Contains('<')) // Nếu đã có tag màu khác, thay thế nó
                {
                    lines[i] = Regex.Replace(line, "<.*?>", $"<{currentTag}>");
                }
                else // Nếu chưa có, bọc text lại
                {
                    lines[i] = Regex.Replace(line, "\"(.*?)\"", m => $"\"<{currentTag}>{{{{{m.Groups[1].Value}}}}}\"");
                }
                modified = true;
                linesToWrite--;
                if (linesToWrite <= 0) state = ReadState.ReadingData;
            }
        }

        return modified ? string.Join("\r\n", lines) : null;
    }

    public bool ShouldPatch(Dictionary<string, bool> bools, Dictionary<string, float> floats)
    {
        return bools.TryGetValue("colourMods", out bool enabled) && enabled;
    }
}