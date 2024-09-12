using MicroServiceReport.Data;
using MicroServiceReport.Services;
using MicroServiceReport.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

// Enregistrement des services
builder.Services.AddScoped<IReportService, ReportService>();  // Enregistre ReportService
builder.Services.AddScoped<IPatientService, PatientService>();  // Enregistre PatientService
builder.Services.AddScoped<INoteService, NoteService>();  // Enregistre NoteService (car il est Singleton dans ton code)


// Configuration MongoDB et SQL
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("PatientDatabase"));

builder.Services.AddDbContext<SqlDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration des JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["jwt"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                context.NoResult();
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            }
        };
    });

// Ajout de l'autorisation
builder.Services.AddAuthorization();

var app = builder.Build();

// Swagger en mode développement
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Activer CORS avant l'authentification
//app.UseCors("AllowFrontend");

// Middleware pour l'authentification et l'autorisation
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
