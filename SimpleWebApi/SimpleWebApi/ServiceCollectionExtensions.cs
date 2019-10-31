using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleWebApi
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomMetrics(this IServiceCollection services)
        {
            AppMetrics.CreateDefaultBuilder()
                .Configuration
                .Configure(options =>
                {
                    options.Enabled = true;
                    options.ReportingEnabled = true;

                }).Report.ToConsole(options=>
                { 
                    options.FlushInterval= TimeSpan.FromSeconds(5);
                });

            return services.AddMetrics();
        }
    }
}
