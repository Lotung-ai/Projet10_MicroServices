using MicroFrontEnd.Controllers;
using MicroFrontEnd.Services.Interfaces;
using MicroFrontEnd.Services;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(); 
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();


builder.Services.AddScoped<IFrontService, FrontService>();

var app = builder.Build();

// Middleware personnalisé pour supprimer les cookies au démarrage
/*app.Use(async (context, next) =>
{
    if (context.Request.Cookies.ContainsKey("jwt"))
    {
        context.Response.Cookies.Delete("jwt");
    }

    await next();
});*/

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
