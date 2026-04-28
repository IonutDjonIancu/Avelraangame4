using Avelraangame4.Components;
using Services.Persistence;
using Statics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient<ICloudflareKvService, CloudflareKvService>();
builder.Services.AddSingleton<IGameStateService, GameStateService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var gameState = scope.ServiceProvider.GetRequiredService<IGameStateService>();
    var env = app.Environment;
    var key = env.IsDevelopment() ? Helpers.SnapshotTest : Helpers.SnapshotProd;
    await gameState.LoadSnapshotIntoMemory(key);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
