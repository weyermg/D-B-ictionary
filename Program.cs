using Avalonia;
using AvaloniaUI.DiagnosticsSupport.Avalonia;

namespace D_B_ictionary;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithDeveloperTools()
        .LogToTrace();
}