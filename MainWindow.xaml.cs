using LibBundledGGPK3;
using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json; // Cần thêm thư viện này
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace PoeFixer;

public class ColorModsOption : INotifyPropertyChanged
{
    private string _color = string.Empty;
    private string _displayName = string.Empty;

    public string Key { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }

    // Thuộc tính này sẽ nhận giá trị từ JSON nếu có
    public string DisplayName
    {
        get => _displayName;
        set { _displayName = value; OnPropertyChanged(); }
    }

    public string Color
    {
        get => _color;
        set { _color = value; OnPropertyChanged(); }
    }

    // 1. Constructor không tham số: Bắt buộc để JSON nạp dữ liệu mà không bị tính toán lại
    public ColorModsOption() { }

    // 2. Constructor có tham số: Dùng cho danh sách mặc định (GetDefaultMods)
    public ColorModsOption(string key, string color, bool isEnabled)
    {
        Key = key;
        _color = color;
        IsEnabled = isEnabled;
        // Chỉ tính toán tên mặc định khi tạo mới lần đầu
        _displayName = key.Replace("map_", "").Replace("_", " ").ToUpper();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
public partial class MainWindow : Window
{
    public string? GGPKPath { get; set; }
    public static List<ColorModsOption> ColorModList { get; set; } = new();
    // Dictionary vẫn giữ nguyên để các class khác truy cập nhanh
    public static Dictionary<string, ColorModsOption> SelectedColorMods =>
        ColorModList.ToDictionary(x => x.Key, x => x);
    private string configPath = "color_settings.json";
    public MainWindow()
    {
        InitializeComponent();
        LoadSettings(); // Tự động load khi mở ứng dụng
    }
    private void LoadSettings()
    {
        if (File.Exists(configPath))
        {
            try
            {
                string json = File.ReadAllText(configPath);
                ColorModList = JsonSerializer.Deserialize<List<ColorModsOption>>(json) ?? GetDefaultMods();
            }
            catch { ColorModList = GetDefaultMods(); }
        }
        else
        {
            ColorModList = GetDefaultMods();
        }
    }

    public void SaveSettings()
    {
        string json = JsonSerializer.Serialize(ColorModList, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(configPath, json);
    }


    // Đưa danh sách khổng lồ của bạn vào đây để làm "Dự phòng"
    private List<ColorModsOption> GetDefaultMods()
    {
        return new List<ColorModsOption>
        { 
        new ColorModsOption("map_monsters_reflect_%_physical_damage", "yellow", true),
        new ColorModsOption("map_monsters_reflect_%_elemental_damage", "green", true),
        new ColorModsOption("map_player_cannot_expose", "red", true),
        new ColorModsOption("map_players_no_regeneration_including_es", "red", true),
        new ColorModsOption("map_player_non_curse_aura_effect_+%", "red", true),
        new ColorModsOption("map_monsters_avoid_elemental_ailments_%", "red", true),
        new ColorModsOption("map_monsters_cannot_be_leeched_from", "red", true),
        new ColorModsOption("map_monsters_cannot_be_stunned", "red", true),
        new ColorModsOption("map_additional_player_maximum_resistances_%", "red", true),
        new ColorModsOption("map_monsters_are_hexproof", "red", true),
        new ColorModsOption("map_player_cooldown_speed_+%_final", "red", true),
        new ColorModsOption("map_player_life_and_es_recovery_speed_+%_final", "red", true),
        new ColorModsOption("map_ground_orion_meteor", "red", true),
        new ColorModsOption("map_player_create_enemy_meteor_daemon_on_flask_use_%_chance", "red", true),
        new ColorModsOption("map_uber_drowning_orb_ambush", "red", true),
        new ColorModsOption("map_petrificiation_statue_ambush", "red", true),
        new ColorModsOption("map_exarch_traps", "red", true),
        new ColorModsOption("map_packs_have_uber_tentacle_fiends", "red", true),
        new ColorModsOption("map_uber_map_additional_synthesis_boss", "red", true),
        new ColorModsOption("map_monster_add_x_grasping_vines_on_hit", "red", true),
        new ColorModsOption("map_uber_map_player_damage_cycle", "red", true),
        new ColorModsOption("map_player_death_mark_on_rare_unique_kill_ms", "red", true),
        new ColorModsOption("map_uber_sawblades_ambush", "red", true),
        new ColorModsOption("map_rare_monster_volatile_on_death_%", "red", true),
        new ColorModsOption("map_rare_monsters_shaper_touched", "red", true),
        new ColorModsOption("map_supporter_maven_follower", "red", true),
        new ColorModsOption("map_player_global_defences_+%", "red", true),
        new ColorModsOption("map_monsters_base_block_%", "red", true),
        new ColorModsOption("chest_display_explodes_corpses", "red", true),
        new ColorModsOption("chest_display_explosion", "red", true),
        new ColorModsOption("chest_display_freeze", "red", true),
        new ColorModsOption("chest_display_ice_nova", "red", true),
        new ColorModsOption("chest_spawn_rogue_exiles", "red", true),
        new ColorModsOption("all_damage_can_freeze", "red", true),
        new ColorModsOption("apply_petrification_for_X_seconds_on_hit", "red", true),
        new ColorModsOption("base_cold_immunity", "red", true),
        new ColorModsOption("base_fire_immunity", "red", true),
        new ColorModsOption("base_lightning_immunity", "red", true),
        new ColorModsOption("chaos_immunity", "red", true),
        new ColorModsOption("physical_immunity", "red", true),
        new ColorModsOption("cannot_have_life_leeched_from", "red", true),
        new ColorModsOption("cannot_have_mana_leeched_from", "red", true),
        new ColorModsOption("life_mana_es_recovery_rate_+%_per_endurance_charge", "red", true),
        new ColorModsOption("global_defences_+%_per_frenzy_charge", "red", true),
        new ColorModsOption("critical_strike_multiplier_+_per_power_charge", "red", true),
        new ColorModsOption("random_projectile_direction", "red", true),
        new ColorModsOption("chaos_damage_per_minute_while_affected_by_flask", "red", true),
        new ColorModsOption("create_enemy_meteor_daemon_on_flask_use_%_chance", "red", true),
        new ColorModsOption("drain_x_flask_charges_over_time_on_hit_for_6_seconds", "red", true),
        new ColorModsOption("map_item_drop_quantity_+%", "pink", true),
        new ColorModsOption("map_item_drop_rarity_+%", "pink", true),
        new ColorModsOption("chance_%_to_drop_additional_divine_orb", "yellow", true),
        new ColorModsOption("map_boss_additional_divine_orb_to_drop", "yellow", true),
        new ColorModsOption("%_chance_to_duplicate_dropped_currency", "blue", true),
        new ColorModsOption("%_chance_to_duplicate_dropped_divination_cards", "blue", true),
        new ColorModsOption("%_chance_to_duplicate_dropped_scarabs", "blue", true),
        new ColorModsOption("%_chance_to_duplicate_dropped_uniques", "blue", true)
    };
    }

    // Hàm mở cửa sổ cấu hình màu
    private void OpenColorSettings_Click(object sender, RoutedEventArgs e)
    {
        // Kiểm tra xem class ColorSettings có tồn tại không
        ColorSettings settingsWindow = new ColorSettings();
        settingsWindow.Owner = this;
        settingsWindow.ShowDialog();
        // SAU KHI ĐÓNG: Lưu ngay các thay đổi vào file json
        SaveSettings();
        EmitToConsole("New color profile saved..");
    }


    public void EmitToConsole(string text, SolidColorBrush? color = null)
    {
        // Đảm bảo chạy trên luồng UI
        App.Current.Dispatcher.Invoke(() =>
        {
            // Nếu không truyền màu, dùng màu mặc định của giao diện
            if (color == null) color = new SolidColorBrush(Color.FromRgb(203, 213, 225)); // #CBD5E1

            TextRange tr = new TextRange(Console.Document.ContentEnd, Console.Document.ContentEnd);
            tr.Text = $"[{DateTime.Now:HH:mm:ss}] {text}\r";
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, color);

            Console.ScrollToEnd();
        });
    }

    private void RestoreExtractedAssets(object sender, RoutedEventArgs e)
    {
        if (GGPKPath == null)
        {
            EmitToConsole("GGPK is not selected.");
            return;
        }

        // Check if ggpk extension is .ggpk.
        if (GGPKPath.EndsWith(".ggpk"))
        {
            BundledGGPK ggpk = new(GGPKPath);
            PatchManager manager = new(ggpk.Index, this);
            int count = manager.RestoreExtractedAssets();
            ggpk.Dispose();
            EmitToConsole($"{count} assets restored.");
        }

        if (GGPKPath.EndsWith(".bin"))
        {
            LibBundle3.Index index = new(GGPKPath);
            PatchManager manager = new(index, this);
            int count = manager.RestoreExtractedAssets();
            index.Dispose();
            EmitToConsole($"{count} assets restored.");
        }
    }

    private async void ExtractVanillaAssets(object sender, RoutedEventArgs e)
    {
        if (GGPKPath == null)
        {
            EmitToConsole("GGPK is not selected.");
            return;
        }

        // Vô hiệu hóa nút để tránh nhấn chồng chéo
        var btn = sender as System.Windows.Controls.Button;
        if (btn != null) btn.IsEnabled = false;

        EmitToConsole("--- Bắt đầu trích xuất tài nguyên ---");

        try
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                int count = 0;

                // Xử lý cho file .ggpk
                if (GGPKPath.EndsWith(".ggpk"))
                {
                    using (BundledGGPK ggpk = new(GGPKPath))
                    {
                        FileExtractor extractor = new(ggpk.Index);
                        // Gửi log về UI qua Dispatcher
                        count = extractor.ExtractFiles(line =>
                            Dispatcher.Invoke(() => EmitToConsole(line))
                        );
                    }
                }
                // Xử lý cho file .bin
                else if (GGPKPath.EndsWith(".bin"))
                {
                    using (LibBundle3.Index index = new(GGPKPath))
                    {
                        FileExtractor extractor = new(index);
                        count = extractor.ExtractFiles(line =>
                            Dispatcher.Invoke(() => EmitToConsole(line))
                        );
                    }
                }

                // Thông báo kết quả cuối cùng
                Dispatcher.Invoke(() => EmitToConsole($"[Hoàn tất] {count} assets extracted."));
            });
        }
        catch (Exception ex)
        {
            EmitToConsole($"[Lỗi] {ex.Message}");
        }
        finally
        {
            if (btn != null) btn.IsEnabled = true;
        }
    }

    private void SelectGGPK(object sender, RoutedEventArgs e)
    {
        PathTextBox.Text = GGPKPath;
        // Open file dialogue to select either a .ggpk or .bin file.
        OpenFileDialog dlg = new()
        {
            DefaultExt = ".ggpk",
            Filter = "GGPK Files (*.ggpk, *.bin)|*.ggpk;*.bin"
        };

        if (dlg.ShowDialog() == true)
        {
            this.GGPKPath = dlg.FileName;
            // Cập nhật lên giao diện
            PathTextBox.Text = this.GGPKPath;
            EmitToConsole($"GGPK selected: {this.GGPKPath}");
        }
    }

    private void PatchGGPK(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(GGPKPath))
        {
            EmitToConsole("GGPK is not selected.");
            return;
        }

        // 1. Khóa giao diện và hiện trạng thái Loading
        SetLoadingState(true);
        // Ép giao diện cập nhật ngay lập tức trước khi luồng chính bị treo
        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => { }));
        EmitToConsole("Bắt đầu Patch (Giao diện sẽ tạm dừng phản hồi)...");

        Stopwatch sw = Stopwatch.StartNew();

        // 2. Thu thập dữ liệu cấu hình (Chạy trực tiếp trên UI nên cực kỳ an toàn)
        var colorSnapshot = ColorModList.ToDictionary(
            x => x.Key,
            x => (x.Color, x.IsEnabled)
        );
        Colour.ConfigSnapshot = colorSnapshot;

        try
        {
            int totalPatched = 0;

            // 3. Chạy trực tiếp trên luồng này luôn
            if (GGPKPath.EndsWith(".ggpk"))
            {
                using (BundledGGPK ggpk = new(GGPKPath))
                {
                    PatchManager manager = new(ggpk.Index, this);
                    // QUAN TRỌNG: Phải gán kết quả trả về vào biến totalPatched
                    totalPatched = manager.Patch();

                }
            }
            else if (GGPKPath.EndsWith(".bin"))
            {
                using (LibBundle3.Index index = new(GGPKPath))
                {
                    PatchManager manager = new(index, this);
                    manager.CollectSettings();
                    totalPatched = manager.Patch();
                   
                }
            }

            sw.Stop();
            EmitToConsole($"[Hoàn tất] {totalPatched} assets patched in {(int)sw.Elapsed.TotalMilliseconds}ms.");
        }
        catch (Exception ex)
        {
            EmitToConsole($"[Lỗi Patch] {ex.Message}", Brushes.Red);
        }
        finally
        {
            // 4. Mở khóa giao diện
            SetLoadingState(false);
        }
    }
    private void SetLoadingState(bool isLoading)
    {
        // Cập nhật giao diện qua Dispatcher để đảm bảo an toàn luồng
        Dispatcher.Invoke(() => {
            var sb = (System.Windows.Media.Animation.Storyboard)this.Resources["RotationAnimation"];

            if (sb != null)
            {
                if (isLoading)
                {
                    PatchButton.IsEnabled = false;
                    LoadingSpinner.Visibility = Visibility.Visible;
                    ButtonText.Text = "DANG PATCH...";
                    sb.Begin(); // Chạy hiệu ứng xoay
                }
                else
                {
                    sb.Stop(); // Dừng hiệu ứng xoay
                    PatchButton.IsEnabled = true;
                    LoadingSpinner.Visibility = Visibility.Collapsed;
                    ButtonText.Text = "PATCH";
                }
            }
        });
    }

    private void colourMods_Checked(object sender, RoutedEventArgs e)
    {

    }
}