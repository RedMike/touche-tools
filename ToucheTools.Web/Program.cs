using ToucheTools.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IModelStorageService, ModelStorageService>();
builder.Services.AddSingleton<IImagePackageStorageService, ImagePackageStorageService>();

builder.Services.AddSingleton<IFileProcessingService, FileProcessingService>();
builder.Services.AddSingleton<IImageRenderingService, MemoryImageRenderingService>();
builder.Services.AddSingleton<IImagePackageProcessingService, ImagePackageProcessingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();