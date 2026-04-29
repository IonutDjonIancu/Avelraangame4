using Avelraangame4.Components;
using Services.Auth;
using Services.Persistence;
using Services.Validation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient<ICloudflareKvService, CloudflareKvService>();
builder.Services.AddSingleton<IGameStateService, GameStateService>();

builder.Services.AddScoped<IValidationsService, ValidationsService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication("AvelraanCookie")
    .AddCookie("AvelraanCookie", options =>
    {
        options.Cookie.Name = "AvelraanSession";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/not-found";
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSession();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// load Gamestate into memory
var gameState = app.Services.GetRequiredService<IGameStateService>();
await gameState.LoadSnapshotIntoMemory();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapControllers();

app.Run();
