if (args.Contains("--ingest", StringComparer.Ordinal))
{
    Ingester.Parser.Skim();
    return;
}

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapGet("/api/health", () => TypedResults.Ok(new { status = "ok" }));
app.Run();
