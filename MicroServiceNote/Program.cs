using MicroServiceNote.Data;
using MicroServiceNote.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Configure Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<PatientDbSettings>(
    builder.Configuration.GetSection("PatientDatabase"));

builder.Services.AddSingleton<NoteService>();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Servez les fichiers statiques
app.UseStaticFiles();

app.UseRouting();

// Authentification et Autorisation
app.UseAuthentication();
app.UseAuthorization();

// Mappage des contrôleurs d'API
app.MapControllers();

app.Run();
