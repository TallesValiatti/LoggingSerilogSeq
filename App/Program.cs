using App.Logging;
using Elastic.Apm.NetCoreAll;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Logging
builder.AddCustomLogging();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAllElasticApm(builder.Configuration);

app.MapGet("/logging", (ILogger<Program> logger) =>
  {
    logger.LogTrace("This is a trace log");
    logger.LogDebug("This is a debug log");
    logger.LogInformation("This is an information log");
    logger.LogWarning("This is a warning log");
    logger.LogError("This is an error log");
    logger.LogCritical("This is a critical log");

    try
    {
      throw new Exception("*** Error ***");
    }
    catch (Exception e)
    {
      logger.LogCritical(e, "This is an exception!");
    } 
    
    return Results.Ok();
  })
.WithName("LoggingEndpoint")
.WithOpenApi();

app.Run();