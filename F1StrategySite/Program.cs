using F1StrategySite.Components;
using F1StrategySite.Data;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Ensure static web assets (including CSS isolation) are served when running from source in any environment
builder.WebHost.UseStaticWebAssets();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapRazorPages();
app.MapControllers();
app.Run();
