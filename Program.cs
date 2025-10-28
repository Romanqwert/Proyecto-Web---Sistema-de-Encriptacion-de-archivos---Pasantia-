using EncriptacionApi.Application.Interfaces;
using EncriptacionApi.Application.Services;
using EncriptacionApi.Infrastructure.Data;
using EncriptacionApi.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using DotNetEnv;

namespace EncriptacionApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Cargar variables del .env
            Env.Load();

            var builder = WebApplication.CreateBuilder(args);
            var config = builder.Configuration;

            // Define un nombre para tu pol�tica de CORS
            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

            // Agrega el servicio de CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                                  policy =>
                                  {
                                      // 3. Especifica el dominio de tu frontend
                                      policy.WithOrigins("https://frontend-encriptacion.vercel.app")
                                            .AllowAnyHeader()
                                            .AllowAnyMethod();
                                  });
            });


            // --- 1. Registrar Servicios (Inyecci�n de Dependencias) ---

            // Servicios de Aplicaci�n
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IHistorialService, Application.Interfaces.HistorialService>();
            builder.Services.AddSingleton<IEncryptionService, EncryptionService>(); // Singleton es eficiente aqu�
            builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

            // Repositorios de Infraestructura
            builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            builder.Services.AddScoped<IArchivoRepository, ArchivoRepository>();

            // DbContext
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION");
            builder.Services.AddDbContext<AppDbContext>(options => 
                options.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(8, 0, 36)) // Ajusta tu versi�n de MySQL
                )
            );

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // --- 2. Configurar Swagger/OpenAPI con Soporte para JWT ---
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Encriptacion API", Version = "v1" });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Por favor ingrese 'Bearer' seguido de un espacio y el token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            // --- 3. Configurar Autenticaci�n JWT ---
            var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET");
            var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
            var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                    };
                });

            // Servicio para acceder al HttpContext (ej. para la IP)
            builder.Services.AddHttpContextAccessor();

            // --- 4. Construir y Configurar el Pipeline HTTP ---
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors(MyAllowSpecificOrigins); // <-- A�adido

            // Habilitar Autenticaci�n y Autorizaci�n
            app.UseAuthentication(); // <-- A�adido
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}


//using Microsoft.EntityFrameworkCore;
//using DotNetEnv;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.IdentityModel.Tokens;
//using System.Text;
//using EncriptacionApi.Infrastructure.Data;

//namespace EncriptacionApi
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            // Cargar variables del .env
//            Env.Load();

//            var builder = WebApplication.CreateBuilder(args);

//            // Add services to the container.

//            builder.Services.AddControllers();
//            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//            builder.Services.AddEndpointsApiExplorer();
//            builder.Services.AddSwaggerGen();

//            // Conexion a la base de datos
//            builder.Services.AddDbContext<AppDbContext>(options => 
//                options.UseMySql(
//                    Environment.GetEnvironmentVariable("DB_CONNECTION"),
//                    new MySqlServerVersion(new Version(8, 0, 36))
//                )
//            );

//            // JWT
//            var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET");
//            var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
//            var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

//            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//                .AddJwtBearer(options =>
//                {
//                    options.TokenValidationParameters = new TokenValidationParameters
//                    {
//                        ValidateIssuer = true,
//                        ValidateAudience = true,
//                        ValidateLifetime = true,
//                        ValidateIssuerSigningKey = true,
//                        ValidIssuer = jwtIssuer,
//                        ValidAudience = jwtAudience,
//                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
//                    };
//                });

//            var app = builder.Build();

//            // Configure the HTTP request pipeline.
//            if (app.Environment.IsDevelopment())
//            {
//                app.UseSwagger();
//                app.UseSwaggerUI();
//            }

//            app.UseHttpsRedirection();

//            app.UseAuthorization();


//            app.MapControllers();

//            app.Run();
//        }
//    }
//}
