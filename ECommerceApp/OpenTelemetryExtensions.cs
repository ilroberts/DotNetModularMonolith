// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ECommerceApp;

public static class OpenTelemetryExtensions
{
public static IServiceCollection AddOpenTelemetryConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var openTelemetryConfig = configuration.GetSection("OpenTelemetry");

        services.AddOpenTelemetry()
            .WithMetrics(builder =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(
                            serviceName: openTelemetryConfig["ServiceName"],
                            serviceVersion: openTelemetryConfig["ServiceVersion"]))
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation();

                // Conditional instrumentation
                if (openTelemetryConfig.GetValue<bool>("Instrumentation:AspNetCore"))
                    builder.AddAspNetCoreInstrumentation();

                if (openTelemetryConfig.GetValue<bool>("Instrumentation:HttpClient"))
                    builder.AddHttpClientInstrumentation();

                // Add exporters based on configuration
                var prometheusEnabled = openTelemetryConfig.GetValue<bool>("Exporters:Prometheus:Enabled");
                var azureMonitorEnabled = openTelemetryConfig.GetValue<bool>("Exporters:AzureMonitor:Enabled");

                if (prometheusEnabled)
                {
                    builder.AddPrometheusExporter();
                }

                if (azureMonitorEnabled)
                {
                    var connectionString = openTelemetryConfig["Exporters:AzureMonitor:ConnectionString"];
                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        builder.AddAzureMonitorMetricExporter(options =>
                        {
                            options.ConnectionString = connectionString;
                        });
                    }
                }
            })
            .WithTracing(builder =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(
                            serviceName: openTelemetryConfig["ServiceName"],
                            serviceVersion: openTelemetryConfig["ServiceVersion"]));

                if (openTelemetryConfig.GetValue<bool>("Instrumentation:AspNetCore"))
                    builder.AddAspNetCoreInstrumentation();

                if (openTelemetryConfig.GetValue<bool>("Instrumentation:HttpClient"))
                    builder.AddHttpClientInstrumentation();

                if (openTelemetryConfig.GetValue<bool>("Instrumentation:SqlClient"))
                    builder.AddSqlClientInstrumentation();

                var azureMonitorEnabled = openTelemetryConfig.GetValue<bool>("Exporters:AzureMonitor:Enabled");
                if (azureMonitorEnabled)
                {
                    var connectionString = openTelemetryConfig["Exporters:AzureMonitor:ConnectionString"];
                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        builder.AddAzureMonitorTraceExporter(options =>
                        {
                            options.ConnectionString = connectionString;
                        });
                    }
                }
            });

        return services;
    }
}
