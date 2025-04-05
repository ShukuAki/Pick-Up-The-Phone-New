using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PUTP2.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Http;

namespace PUTP2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddControllersWithViews();
                    services.AddScoped<PlaylistStorageService>();
                    services.Configure<IISServerOptions>(options =>
                    {
                        options.MaxRequestBodySize = 52428800; // 50MB in bytes
                    });
                    services.Configure<FormOptions>(options =>
                    {
                        options.MultipartBodyLengthLimit = 52428800; // 50MB in bytes
                    });
                });
    }
}
