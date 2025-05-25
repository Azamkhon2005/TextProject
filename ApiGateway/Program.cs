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
                        .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true) // ��������� ocelot.json
                        .AddEnvironmentVariables();
                })
                .ConfigureServices(services =>
                {
                    services.AddOcelot(); // ��������� ������� Ocelot
                    // ���� ��� ����� ������ �������, ��������, ��� ��������������, CORS � �.�., ���������� �� �����
                    // services.AddControllers(); // �� �����, ���� API Gateway ������ ��� ��������
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure(app =>
                    {
                        // ������� middleware �����
                        // app.UseRouting(); // �� ����������, ���� Ocelot - ������������, ��� ������������ �������

                        // app.UseAuthentication(); // ���� ����� ��������������
                        // app.UseAuthorization();

                        // app.UseEndpoints(endpoints =>
                        // {
                        //     endpoints.MapControllers(); // �� �����, ���� ��� ����� ������������ � Gateway
                        // });

                        // ���������� Ocelot middleware
                        // ����������� app.UseOcelot().Wait() ��� await app.UseOcelot()
                        // ��� .NET 6+ � ������������ Main, ����� ������������ await
                        app.UseOcelot().Wait(); // ��� await app.UseOcelot(); ���� Main �����������
                                                // ���� Main ����������, Wait() ����������, ����� �������� ������� � ����������.
                                                // � ������ ������, �.�. Run() ���������, Wait() ����� ��.
                                                // ���� �� �� ������ await app.RunAsync(), �� � ����� ��� �� await.
                    });
                });
    }
}