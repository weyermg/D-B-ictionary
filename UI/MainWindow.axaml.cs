using Avalonia.Controls;

namespace D_B_ictionary;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var connection = new ElasticSearch.ElasticSearchConnection();
        var dialogService = new AvaloniaAddWordDialogService(this);
        DataContext = new MainWindowViewModel(connection, dialogService);
    }
}