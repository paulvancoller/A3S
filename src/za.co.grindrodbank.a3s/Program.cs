/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using NLog;
using NLog.Web;

namespace za.co.grindrodbank.a3s
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            StartApp(args);
        }

        public static void StartApp(string[] args)
        {
            NLogBuilder.ConfigureNLog("nlog.config");

            try
            {
                CreateWebHostBuilder(args).Build().Run();
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseNLog();
    }
}
