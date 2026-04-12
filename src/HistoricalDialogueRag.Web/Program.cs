using HistoricalDialogueRag.Web.Cli;
using HistoricalDialogueRag.Web.Composition;
using HistoricalDialogueRag.Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddCoreServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

if (await app.TryRunCliCommandAsync(args))
    return;

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapHealthEndpoints();
app.MapFigureEndpoints();
app.MapDialogueEndpoints();
app.MapRazorPages();

app.Run();