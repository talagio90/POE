namespace PoeFixer;

public class CameraZoom : IPatch
{
    public float zoomLevel = 1;

    // Chỉ định nghĩa file duy nhất cần sửa
    public string Extension => "*.ot";

    public string[] FilesToPatch => [
        "metadata/characters/character.ot"
    ];

    // Để trống vì bạn chỉ muốn sửa file cụ thể trên
    public string[] DirectoriesToPatch => [];

    public bool ShouldPatch(Dictionary<string, bool> bools, Dictionary<string, float> floats)
    {
        bools.TryGetValue("ZoomEnabled", out bool enabled);
        floats.TryGetValue("zoomSlider", out zoomLevel);
        return enabled;
    }

    public string? PatchFile(string text)
    {
        // Định dạng zoomLevel sang chuỗi (VD: 2.4)
        string zoomLevelString = zoomLevel.ToString("F1").Replace(',', '.');

        // Tách dòng để xử lý chuẩn xác
        List<string> lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();

        // Kiểm tra xem đã patch chưa để tránh chèn đè nhiều lần
        int existingIndex = lines.FindIndex(x => x.Contains("CreateCameraZoomNode"));

        if (existingIndex != -1)
        {
            // Nếu đã có lệnh zoom, cập nhật lại giá trị mới từ Slider
            lines[existingIndex] = $"\ton_initial_position_set = \"CreateCameraZoomNode(5000.0, 5000.0, {zoomLevelString});\"";
        }
        else
        {
            // Nếu chưa có, tìm dòng "team = 1" để chèn vào ngay sau đó
            int teamIndex = lines.FindIndex(x => x.Contains("team = 1"));
            if (teamIndex != -1)
            {
                lines.Insert(teamIndex + 1, $"\ton_initial_position_set = \"CreateCameraZoomNode(5000.0, 5000.0, {zoomLevelString});\"");
            }
            else
            {
                // Nếu không tìm thấy team = 1 (trường hợp hiếm), không trả về null để tránh PatchManager bỏ qua
                return text;
            }
        }

        return string.Join("\r\n", lines);
    }
}