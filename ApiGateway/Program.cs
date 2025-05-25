using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace ApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config
                        .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true) // Добавляем ocelot.json
                        .AddEnvironmentVariables();
                })
                .ConfigureServices(services =>
                {
                    services.AddOcelot(); // Добавляем сервисы Ocelot
                    // Если вам нужны другие сервисы, например, для аутентификации, CORS и т.д., добавляйте их здесь
                    // services.AddControllers(); // Не нужен, если API Gateway только для роутинга
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure(app =>
                    {
                        // Порядок middleware важен
                        // app.UseRouting(); // Не обязателен, если Ocelot - единственное, что обрабатывает запросы

                        // app.UseAuthentication(); // Если будет аутентификация
                        // app.UseAuthorization();

                        // app.UseEndpoints(endpoints =>
                        // {
                        //     endpoints.MapControllers(); // Не нужен, если нет своих контроллеров в Gateway
                        // });

                        // Используем Ocelot middleware
                        // Используйте app.UseOcelot().Wait() или await app.UseOcelot()
                        // Для .NET 6+ и асинхронного Main, лучше использовать await
                        app.UseOcelot().Wait(); // ИЛИ await app.UseOcelot(); если Main асинхронный
                                                // Если Main синхронный, Wait() безопаснее, чтобы избежать проблем с контекстом.
                                                // В данном случае, т.к. Run() блокирует, Wait() здесь ОК.
                                                // Если бы вы делали await app.RunAsync(), то и здесь был бы await.
                    });
                });
    }
}