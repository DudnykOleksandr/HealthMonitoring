using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Extensions.DependencyInjection;
using App.Metrics.Extensions.HealthChecks;
using App.Metrics.Reporting.InfluxDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SimpleWebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddHealthChecks()
                .AddCheck<RandomHealthCheck>("random")
                .AddCheck("ping google", () =>
                {
                    try
                    {
                        using (var ping = new Ping())
                        {
                            var reply = ping.Send("www.google.com");
                            if (reply.Status != IPStatus.Success)
                            {
                                return HealthCheckResult.Unhealthy();
                            }

                            if (reply.RoundtripTime > 100)
                            {
                                return HealthCheckResult.Degraded();
                            }

                            return HealthCheckResult.Healthy();
                        }
                    }
                    catch
                    {
                        return HealthCheckResult.Unhealthy();
                    }
                });

            var metrics = AppMetrics.CreateDefaultBuilder()
                .Configuration
                .Configure(options => { })
                //.Report.ToTextFile(@"D:\metrics.txt", TimeSpan.FromSeconds(5));
                .Report.ToInfluxDb(
                    options =>
                    {
                        var influxMetricsReportingSettings = Configuration.GetSection("InfluxReporting").Get<InfluxDbOptions>();
                        influxMetricsReportingSettings.Database = "Database";
                        options.InfluxDb = influxMetricsReportingSettings;
                        options.FlushInterval = TimeSpan.FromSeconds(5);
                    });

            services
                .AddMetrics(metrics)
                .AddMetricsReportingHostedService()
                .AddMetricsTrackingMiddleware()
                .AddSingleton<IHealthCheckPublisher, AppMetricsHealthCheckPublisher>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMetricsAllMiddleware();

            app.UseHealthChecks("/health");

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public class RandomHealthCheck : IHealthCheck
        {
            public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
            {
                if (DateTime.UtcNow.Minute % 2 == 0)
                {
                    return Task.FromResult(HealthCheckResult.Healthy());
                }

                return Task.FromResult(HealthCheckResult.Unhealthy(description: "failed"));
            }

        }
    }
}
