using MicroFrontEnd.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(); 
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Optionnel : Configuration de la protection contre les attaques CSRF (Antiforgery)
// Si vous avez besoin de configurer des options spécifiques
// builder.Services.AddAntiforgery(options =>
// {
//     options.Cookie.Name = "YourAntiforgeryCookieName";
//     options.FormFieldName = "YourAntiforgeryFormFieldName";
//     // Configurez d'autres options si nécessaire
// });
// Add CORS policy to allow all origins (you can restrict this as needed)
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
var app = builder.Build();
// Use CORS policy
app.UseCors("CorsPolicy");

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
