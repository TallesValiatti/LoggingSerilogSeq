using Elastic.Channels;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using Serilog.Sinks.SystemConsole.Themes;

namespace App.Logging;

public static class LoggerExtensions
{
    public static WebApplicationBuilder AddCustomLogging(this WebApplicationBuilder builder)
    {
       
        // Create the logger configuration
        var loggerConfig = new LoggerConfiguration();
        
        // MinimumLevel
        loggerConfig.MinimumLevel.Information();
        
        // Microsoft minimum log level
        loggerConfig.MinimumLevel.Override("Microsoft", LogEventLevel.Error);
        loggerConfig.MinimumLevel.Override("Elastic.Apm", LogEventLevel.Error);

        // Exceptions details
        loggerConfig.Enrich.WithExceptionDetails();
        
        // Application name
        loggerConfig.Enrich.WithProperty("Application", "my-web-api");
        
        // Add console
        loggerConfig.WriteTo.Console(theme: SystemConsoleTheme.Colored);
        
        // Add SEQ
        loggerConfig.WriteTo.Seq("https://app-seq-prod-eastus.azurewebsites.net/", apiKey: "<you-api-key>");
        
        // Add File
        loggerConfig.WriteTo.File("log.txt", rollingInterval: RollingInterval.Day);
        
        // Add Azure application insights
        loggerConfig.WriteTo.ApplicationInsights("<you-instrumentation-key>", new TraceTelemetryConverter());
        
        // Add ELK 
        loggerConfig.WriteTo.ElasticCloud(
            cloudId:"<your-cloud-id>",
            apiKey: "<you-api-key>",
            opts =>
            {
                opts.DataStream = new DataStreamName("my-logs", "app", "prod"); //logs-my-app-dev
                opts.BootstrapMethod = BootstrapMethod.Failure;
                opts.ConfigureChannel = channelOpts =>
                {
                    channelOpts.BufferOptions = new BufferOptions
                    {
                        ExportMaxConcurrency = 10
                    };
                };
            });
        
        // Create logger
        var logger = loggerConfig.CreateLogger();
        
        // Clear providers
        builder.Logging.ClearProviders();
        
        // Add Serilog
        builder.Logging.AddSerilog(logger);
        
        return builder;
    }
}