using InternalTrainingSystem.WebApp.Helpers;
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

builder.Services.AddHttpContextAccessor();

// Configure HttpClient for API calls
var baseUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl")
             ?? "https://localhost:7001";

Action<HttpClient> configureClient = client =>
{
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
};

builder.Services.AddHttpClient<IUserService, UserService>(configureClient);
builder.Services.AddHttpClient<ICourseService, CourseService>(configureClient);
builder.Services.AddHttpClient<INotificationService, NotificationService>(configureClient);

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICourseEnrollmentService, CourseEnrollmentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

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

app.UseRouting();

app.UseAuthorization();

// Configure custom routing with kebab-case support
app.MapControllerRoute(
    name: "kebab-case",
    pattern: "{controller:slugify}/{action:slugify}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Initialize Utilities with configuration
Utilities.Initialize(app.Configuration);

app.Run();
