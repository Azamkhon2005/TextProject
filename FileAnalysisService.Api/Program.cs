using Microsoft.EntityFrameworkCore;
using FileAnalysisService.Infrastructure.Persistence;
using FileAnalysisService.Infrastructure.HttpClients;
using FileAnalysisService.Application.Services;

namespace FileAnalysisService.Api
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
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Version = "v1",
                    Title = "File Analysis Service API",
                    Description = "API for analyzing text files and retrieving results."
                });
            });

            var connectionString = configuration.GetConnectionString("PostgresConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'PostgresConnection' not found or is empty.");
            }
            services.AddDbContext<FileAnalysisDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: environment.IsDevelopment() ? 2 : 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
        npgsqlOptions.MigrationsAssembly("FileAnalysisService.Infrastructure");
    }));

            services.AddScoped<AnalysisAppService>();
            var fileStoringServiceConfig = configuration.GetSection(FileStoringServiceOptions.SectionName).Get<FileStoringServiceOptions>();
            if (fileStoringServiceConfig == null || string.IsNullOrWhiteSpace(fileStoringServiceConfig.BaseUrl))
            {
                throw new InvalidOperationException($"Configuration for '{FileStoringServiceOptions.SectionName}' is missing or BaseUrl is not set.");
            }

            services.AddHttpClient<IFileStoringServiceClient, FileStoringServiceClient>(client =>
            {
                client.BaseAddress = new Uri(fileStoringServiceConfig.BaseUrl);
            });
            
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });
        }

        private static void ConfigureMiddleware(WebApplication app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "File Analysis Service API V1"));
            }
            else
            {
                app.UseExceptionHandler("/error");
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
                    logger.LogInformation("Attempting to apply FileAnalysisDbContext migrations...");
                    var dbContext = services.GetRequiredService<FileAnalysisDbContext>();
                    dbContext.Database.Migrate();
                    logger.LogInformation("FileAnalysisDbContext migrations applied successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the FileAnalysisDbContext.");
                }
            }
        }
    }
}