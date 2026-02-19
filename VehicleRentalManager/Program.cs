using VehicleRentalManager.Components;
using VehicleRentalManager.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;

// Load environment variables from the parent directory to keep secrets separate from the project source.
DotNetEnv.Env.Load("../.env");

var builder = WebApplication.CreateBuilder(args);

// ── Razor Pages (ExternalLogin + ExternalLoginCallback) ───────────────────────
builder.Services.AddRazorPages();

// ── Blazor ────────────────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ── MongoDB services ──────────────────────────────────────────────────────────
// MongoClient is thread-safe and handles connection pooling internally — safe as Singleton.
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddSingleton<MongoContext>();

// Register concrete services AND their interfaces so Blazor pages can inject IClientService, etc.
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IReservationService, ReservationService>();

builder.Services.AddControllers();

// ── JWT + Auth services ───────────────────────────────────────────────────────
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddHttpContextAccessor();

// ── Authentication ────────────────────────────────────────────────────────────
// Configure dual authentication: Cookies for local persistence, Google for the external identity challenge.
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme          = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    // Fail fast if configuration is missing to prevent runtime surprises.
    var secretKey = builder.Configuration["Jwt:SecretKey"]
        ?? throw new InvalidOperationException("Jwt:SecretKey not configured.");

    // Enforce strict token validation; issuer/audience checks ensure the token is meant for this specific app.
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = builder.Configuration["Jwt:Issuer"]   ?? "VehicleRentalManager",
        ValidAudience            = builder.Configuration["Jwt:Audience"] ?? "VehicleRentalManager",
        IssuerSigningKey         = new SymmetricSecurityKey(
                                       Encoding.UTF8.GetBytes(secretKey))
    };
})
.AddGoogle(options =>
{
    // Ensure OAuth credentials are present at startup.
    options.ClientId     = builder.Configuration["GoogleOAuth:ClientId"]
        ?? throw new InvalidOperationException("GoogleOAuth:ClientId not configured.");
    options.ClientSecret = builder.Configuration["GoogleOAuth:ClientSecret"]
        ?? throw new InvalidOperationException("GoogleOAuth:ClientSecret not configured.");
    options.CallbackPath = "/auth/google-callback";
});

builder.Services.AddAuthorization();

// ── Blazor auth state (reads JWT cookie) ──────────────────────────────────────
// Register the custom provider to propagate the JWT from the HTTP cookie into the Blazor SignalR circuit.
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>(
    sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddCascadingAuthenticationState();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Azure's internal IPs — clear defaults to trust the proxy
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine("===== EXCEPTION GLOBAL =====");
        Console.WriteLine(ex.ToString());
        Console.WriteLine("===== FIM EXCEPTION GLOBAL =====");
        throw;
    }
});

// ── HTTP pipeline ─────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseStatusCodePagesWithReExecute("/not-found");

// Manually handle logout to ensure the custom 'jwt' cookie is deleted.
app.MapGet("/auth/logout-redirect", (HttpContext ctx) =>
{
    ctx.Response.Cookies.Append("jwt", "", new CookieOptions
    {
        Expires  = DateTimeOffset.UtcNow.AddDays(-1),
        HttpOnly = true,
        Secure   = true,
        SameSite = SameSiteMode.Lax
    });
    ctx.Response.Redirect("/auth");
}).AllowAnonymous();

// ── Route mappings ────────────────────────────────────────────────────────────
app.MapRazorPages();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();

app.MapStaticAssets();

app.Run();