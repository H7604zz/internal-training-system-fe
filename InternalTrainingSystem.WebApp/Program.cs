using InternalTrainingSystem.WebApp.Helpers;
using InternalTrainingSystem.WebApp.Middleware;
using InternalTrainingSystem.WebApp.Services.Implement;
using InternalTrainingSystem.WebApp.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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

// Configure HttpClient for API calls
builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
{
    var baseUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl") 
                 ?? "https://localhost:7001"; // Default API URL
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICourseEnrollmentService, CourseEnrollmentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IClassService, ClassService>();

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
