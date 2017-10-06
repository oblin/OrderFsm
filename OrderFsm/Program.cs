using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderFsm.Models;
using System;
using System.IO;

namespace OrderFsm
{
    class Program
    {
        private static IConfigurationRoot _config;

        static void Main(string[] args)
        {
            SetupConfiguration();
            var services = ConfigureServices();

            var context = services.GetService<OrderContext>();
            var repository = new StateRepository(context);

            var orderProcess = new OrderProcess("ord002", repository);
            orderProcess.Start("ServiceA");

            Console.WriteLine(orderProcess.ToDotGraph());

            Console.ReadLine();
        }

        private static void SetupConfiguration()
        {
            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var connection = _config["connection:msm_test"];
            services.AddDbContext<OrderContext>(options => options.UseSqlServer(connection));

            var serviceProvider = services.BuildServiceProvider();

            // config logging
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            loggerFactory.AddDebug();
            return serviceProvider;
        }

    }
}
