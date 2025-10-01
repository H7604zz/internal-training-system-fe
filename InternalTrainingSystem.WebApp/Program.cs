using InternalTrainingSystem.WebApp.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure routing to use kebab-case parameter transformer
builder.Services.Configure<RouteOptions>(options =>
{
    options.ConstraintMap.Add("slugify", typeof(SlugifyParameterTransformer));
});

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
