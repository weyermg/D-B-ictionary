using D_B_ictionary.Api;
using ElasticSearch;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.AddSingleton<ElasticSearchConnection>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => TypedResults.Ok(new { status = "ok" }))
    .WithName("HealthCheck")
    .WithSummary("Check API health");
app.MapWordEndpoints();

var elasticSearch = app.Services.GetRequiredService<ElasticSearchConnection>();
try
{
    await elasticSearch.InitializeAsync(app.Lifetime.ApplicationStopping);
}
catch (Exception exception)
{
    app.Logger.LogError(exception, "Elasticsearch initialization failed; the API will remain available.");
}

app.Run();