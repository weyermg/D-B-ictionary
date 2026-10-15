using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace D_B_ictionary;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ElasticSearch.ElasticSearchConnection _connection;
    private readonly Task _initializationTask;
    private CancellationTokenSource? _searchCancellation;
    private bool _isInitialized;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _message = string.Empty;

    public ObservableCollection<SearchResultViewModel> Results { get; } = new();

    public bool IsMessageVisible => !string.IsNullOrWhiteSpace(Message);

    public MainWindowViewModel(ElasticSearch.ElasticSearchConnection connection)
    {
        _connection = connection;
        _initializationTask = InitializeAsync();
    }

    partial void OnMessageChanged(string value) => OnPropertyChanged(nameof(IsMessageVisible));

    partial void OnSearchTextChanged(string value)
    {
        _searchCancellation?.Cancel();
        Results.Clear();

        if (string.IsNullOrWhiteSpace(value))
        {
            if (_isInitialized)
            {
                Message = string.Empty;
            }
            return;
        }

        var cancellation = new CancellationTokenSource();
        _searchCancellation = cancellation;
        _ = SearchAfterDelayAsync(value.Trim(), cancellation);
    }

    private async Task InitializeAsync()
    {
        Message = "Connecting to Elasticsearch…";
        try
        {
            await _connection.InitializeAsync();
            _isInitialized = true;
            Message = string.Empty;
        }
        catch
        {
            Message = "Elasticsearch initialization failed.";
        }
    }

    private async Task SearchAfterDelayAsync(string searchTerm, CancellationTokenSource cancellation)
    {
        var token = cancellation.Token;
        try
        {
            await Task.Delay(TimeSpan.FromMilliseconds(300), token);
            await _initializationTask;
            token.ThrowIfCancellationRequested();

            if (!_isInitialized)
            {
                Message = "Elasticsearch initialization failed.";
                return;
            }

            Message = "Searching…";
            var matches = await _connection.FuzzyFindWords(searchTerm, "en");
            token.ThrowIfCancellationRequested();

            foreach (var match in matches)
            {
                Results.Add(new SearchResultViewModel(match));
            }

            Message = matches.Count == 0 ? "No matches found." : string.Empty;
        }
        catch (OperationCanceledException)
        {
        }
        catch
        {
            if (!token.IsCancellationRequested)
            {
                Message = "Search failed. Check the Elasticsearch connection.";
            }
        }
        finally
        {
            if (ReferenceEquals(_searchCancellation, cancellation))
            {
                _searchCancellation = null;
            }
            cancellation.Dispose();
        }
    }
}

public sealed class SearchResultViewModel
{
    public string Word { get; }
    public string Language { get; }
    public string Definition { get; }

    public SearchResultViewModel(ElasticSearchWord word)
    {
        Word = word.Word;
        Language = word.Language;
        Definition = string.Join(Environment.NewLine, word.Definition);
    }
}