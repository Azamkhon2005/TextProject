// Например, в проекте ApiGateway или FileStoringService.Api
namespace TextProject // Или другое соответствующее пространство имен
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Конфигурация сервисов
            // builder.Services.AddControllers();
            // ...

            var app = builder.Build();

            // Конфигурация middleware
            // app.MapGet("/", () => "Hello World!");
            // ...

            app.Run();
        }
    }
}