using Avalonia.Controls;

namespace D_B_ictionary;

public interface IAddWordDialogService
{
    Task<ElasticSearchWord?> ShowAsync();
}

public sealed class AvaloniaAddWordDialogService : IAddWordDialogService
{
    private readonly Window _owner;

    public AvaloniaAddWordDialogService(Window owner)
    {
        _owner = owner;
    }

    public async Task<ElasticSearchWord?> ShowAsync()
    {
        var viewModel = new AddWordDialogViewModel();
        var dialog = new AddWordWindow { DataContext = viewModel };
        viewModel.CloseRequested += result => dialog.Close(result);
        return await dialog.ShowDialog<ElasticSearchWord?>(_owner);
    }
}