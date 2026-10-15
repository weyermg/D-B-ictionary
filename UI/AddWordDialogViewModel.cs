using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace D_B_ictionary;

public partial class AddWordDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _word = string.Empty;

    [ObservableProperty]
    private string _language = "en";

    [ObservableProperty]
    private string _etymologyIndex = "1";

    [ObservableProperty]
    private string _definitionIndex = "1";

    [ObservableProperty]
    private string _definitions = string.Empty;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    public bool IsValidationMessageVisible => !string.IsNullOrWhiteSpace(ValidationMessage);

    public event Action<ElasticSearchWord?>? CloseRequested;

    partial void OnValidationMessageChanged(string value) =>
        OnPropertyChanged(nameof(IsValidationMessageVisible));

    [RelayCommand]
    private void Add()
    {
        if (string.IsNullOrWhiteSpace(Word))
        {
            ValidationMessage = "Enter a word.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Language))
        {
            ValidationMessage = "Enter a language code.";
            return;
        }

        if (!int.TryParse(EtymologyIndex, out var etymologyIndex))
        {
            ValidationMessage = "Etymology index must be a whole number.";
            return;
        }

        if (!int.TryParse(DefinitionIndex, out var definitionIndex))
        {
            ValidationMessage = "Definition index must be a whole number.";
            return;
        }

        var definitionLines = Definitions.Split(
            new[] { '\r', '\n' },
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (definitionLines.Length == 0)
        {
            ValidationMessage = "Enter at least one definition.";
            return;
        }

        ValidationMessage = string.Empty;
        CloseRequested?.Invoke(new ElasticSearchWord
        {
            Word = Word.Trim(),
            Language = Language.Trim(),
            etym_index = etymologyIndex,
            def_index = definitionIndex,
            Definition = definitionLines
        });
    }

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(null);
}