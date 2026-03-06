using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PoeFixer;

public partial class ColorSettings : Window
{
    public ColorSettings()
    {
        InitializeComponent();
        ModsListBox.ItemsSource = MainWindow.ColorModList;
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = SearchBox.Text.ToLower().Trim();

        if (string.IsNullOrEmpty(searchText))
        {
            ModsListBox.ItemsSource = MainWindow.ColorModList;
        }
        else
        {
            // Lọc và hiển thị kết quả tìm kiếm
            // Chúng ta dùng ToList() để tạo một danh sách tạm thời cho việc hiển thị
            var filtered = MainWindow.ColorModList
                .Where(m => (m.DisplayName != null && m.DisplayName.ToLower().Contains(searchText)) ||
                            (m.Key != null && m.Key.ToLower().Contains(searchText)))
                .ToList();

            ModsListBox.ItemsSource = filtered;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        // Kiểm tra xem Owner có phải là MainWindow không và gọi hàm Save
        if (this.Owner is MainWindow mainWin)
        {
            mainWin.SaveSettings();
            mainWin.EmitToConsole("The color profile has been saved to a JSON file..");
        }
        else
        {
            // Trường hợp dự phòng nếu Owner không được gán đúng
            ((MainWindow)Application.Current.MainWindow).SaveSettings();
        }

        this.Close();
    }

}