using F1StrategySite.Components;
using F1StrategySite.Data;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Ensure static web assets (including CSS isolation) are served when running from source in any environment
builder.WebHost.UseStaticWebAssets();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        // Surface detailed circuit errors to the browser when developing
        if (builder.Environment.IsDevelopment())
        {
            options.DetailedErrors = true;
        }
    });
builder.Services.AddControllers();
builder.Services.AddRazorPages(options =>
{
    // Razor Pages live under Components/Pages
    options.RootDirectory = "/Components/Pages";
});
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("PredictionLimiter", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10; 
        limiterOptions.Window = TimeSpan.FromMinutes(1); 
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 2;
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();
app.UseRateLimiter();

// Lightweight health and diagnostics endpoints
app.MapGet("/health", () => Results.Ok(new { status = "ok", env = app.Environment.EnvironmentName }))
   .WithName("Health");
app.MapGet("/diag/paths", () => Results.Ok(new
{
    Base = AppContext.BaseDirectory,
    Docs = System.IO.Path.Combine(AppContext.BaseDirectory, "Docs"),
    Model = System.IO.Path.Combine(AppContext.BaseDirectory, "MLModel", "MLModel1.mlnet")
})).WithName("DiagPaths");

app.MapGet("/diag/check", async () =>
{
    var docsDir = Path.Combine(AppContext.BaseDirectory, "Docs");
    var modelPath = Path.Combine(AppContext.BaseDirectory, "MLModel", "MLModel1.mlnet");
    var exists = new
    {
        DocsDirExists = Directory.Exists(docsDir),
        CircuitFileExists = File.Exists(Path.Combine(docsDir, "circut_length.csv")),
        GpLapsExists = File.Exists(Path.Combine(docsDir, "gps_laps.csv")),
        CalendarExists = File.Exists(Path.Combine(docsDir, "grand_prix_calendar.csv")),
        ModelExists = File.Exists(modelPath)
    };

    float? spaLen = null;
    string? loadError = null;
    try
    {
        spaLen = await CircutInfo.GetCircuitLengthAsync("Belgian Grand Prix");
    }
    catch (Exception ex)
    {
        loadError = ex.Message;
    }

    return Results.Ok(new { exists, spaLen, loadError });
}).WithName("DiagCheck");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapRazorPages();
app.MapControllers();
app.Run();
