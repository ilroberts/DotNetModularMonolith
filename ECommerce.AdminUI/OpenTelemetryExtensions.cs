// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ECommerce.AdminUI;

public static class OpenTelemetryExtensions
{
public static IServiceCollection AddOpenTelemetryConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var openTelemetryConfig = configuration.GetSection("OpenTelemetry");

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("AdminUI-Service",
                openTelemetryConfig["ServiceVersion"]))
            .WithMetrics(builder =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(
                            serviceName: openTelemetryConfig["ServiceName"] ?? "AdminUI",
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

                if (!azureMonitorEnabled)
                {
                    return;
                }

                var connectionString = openTelemetryConfig["Exporters:AzureMonitor:ConnectionString"];
                if (!string.IsNullOrEmpty(connectionString))
                {
                    builder.AddAzureMonitorMetricExporter(options =>
                    {
                        options.ConnectionString = connectionString;
                    });
                }
            })
            .WithTracing(builder =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(
                            serviceName: openTelemetryConfig["ServiceName"] ?? "AdminUI",
                            serviceVersion: openTelemetryConfig["ServiceVersion"]));

                if (openTelemetryConfig.GetValue<bool>("Instrumentation:AspNetCore"))
                    builder.AddAspNetCoreInstrumentation();

                if (openTelemetryConfig.GetValue<bool>("Instrumentation:HttpClient"))
                    builder.AddHttpClientInstrumentation();

                if (openTelemetryConfig.GetValue<bool>("Instrumentation:SqlClient"))
                    builder.AddSqlClientInstrumentation(o => o.SetDbStatementForText = true);

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

                builder.AddOtlpExporter(options =>
                {
                    string otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")
                                          ?? "http://jaeger-collector.devops.svc.cluster.local:4317";
                    options.Endpoint = new Uri(otlpEndpoint);
                    options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                });
            });

        return services;
    }
}
