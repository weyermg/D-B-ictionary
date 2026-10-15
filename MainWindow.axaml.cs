using Avalonia.Controls;

namespace D_B_ictionary;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(new ElasticSearch.ElasticSearchConnection());
    }
}