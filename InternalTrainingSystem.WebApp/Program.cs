using InternalTrainingSystem.WebApp.Handlers;
using InternalTrainingSystem.WebApp.Helpers;
using InternalTrainingSystem.WebApp.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });

// Configure routing to use kebab-case parameter transformer
builder.Services.Configure<RouteOptions>(options =>
{
    options.ConstraintMap.Add("slugify", typeof(SlugifyParameterTransformer));
});

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Session timeout after 60 minutes
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Configure Authentication
builder.Services.AddAuthentication("CustomCookie")
    .AddCookie("CustomCookie", options =>
    {
        options.LoginPath = "/dang-nhap";
        options.LogoutPath = "/dang-xuat";
        options.AccessDeniedPath = "/khong-co-quyen";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// Register the authentication handler
builder.Services.AddTransient<AuthenticationHandler>();

// Configure HttpClient factory with base configuration and authentication handler
builder.Services.AddHttpClient("ApiClient", client =>
{
    var baseUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7001";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<AuthenticationHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Use session
app.UseSession();

app.UseRouting();

// Add authentication and authorization
app.UseAuthentication();

// Use custom authentication middleware
app.UseMiddleware<AuthenticationMiddleware>();

app.UseAuthorization();

// Configure custom routing with kebab-case support
app.MapControllerRoute(
    name: "kebab-case",
    pattern: "{controller:slugify}/{action:slugify}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller:slugify=TrangChu}/{action:slugify=Index}/{id?}");

// Initialize Utilities with configuration
Utilities.Initialize(app.Configuration);

app.Run();
