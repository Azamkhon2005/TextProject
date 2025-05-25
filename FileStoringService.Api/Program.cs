
using Microsoft.EntityFrameworkCore;
using FileStoringService.Infrastructure.Persistence;
using FileStoringService.Infrastructure.FileStorage;
using FileStoringService.Application.Services;

namespace FileStoringService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

            var app = builder.Build();

            ConfigureMiddleware(app, app.Environment);

            if (app.Environment.IsDevelopment())
            {
                ApplyMigrations(app);
            }

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddControllers(options =>
            {

            });

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Version = "v1",
                    Title = "File Storing Service API",
                    Description = "API for uploading and retrieving text files."
                });
            });

            var connectionString = configuration.GetConnectionString("PostgresConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'PostgresConnection' not found or is empty in configuration.");
            }
            
            services.AddDbContext<FileStoringDbContext>(options =>
                options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: environment.IsDevelopment() ? 2 : 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
                npgsqlOptions.MigrationsAssembly("FileStoringService.Infrastructure");
            }));

            services.AddScoped<FileAppService>();

            services.Configure<LocalFileSaverOptions>(configuration.GetSection(LocalFileSaverOptions.SectionName));
           
            services.AddSingleton<IFileSaver, LocalFileSaver>();
            
        }

        private static void ConfigureMiddleware(WebApplication app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "File Storing Service API V1");
                    
                });
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
                    });
                });
                app.UseHsts();
            }


            app.UseRouting();

            app.MapControllers();
        }

        private static void ApplyMigrations(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                try
                {
                    logger.LogInformation("Attempting to apply database migrations...");
                    var dbContext = services.GetRequiredService<FileStoringDbContext>();
                    dbContext.Database.Migrate();
                    logger.LogInformation("Database migrations applied successfully (or no migrations to apply).");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the database. Please check DB connection and permissions.");
                }
            }
        }
    }
}