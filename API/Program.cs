using System;
using System.Collections.ObjectModel;
using System.Data;
using API.middelwares;
using Application;
using Confluent.Kafka;
using Domain.Interfaces;
using HealthChecks.UI.Client;
using Infrastructure;
using Infrastructure.Services;
using KafkaWebApiDemo.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

var builder = WebApplication.CreateBuilder(args);
// Read connection string from config
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddApplicationServiceExtensions();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddServiceExtensions(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddConsumerServices(builder.Configuration);



//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();

// Configure Serilog manually
var columnOptions = new ColumnOptions();
columnOptions.AdditionalColumns = new Collection<SqlColumn>
{
    new SqlColumn("ActionId", SqlDbType.UniqueIdentifier),
    new SqlColumn("RequestId", SqlDbType.NVarChar),
    new SqlColumn("Application", SqlDbType.NVarChar)
};


builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ILogEventEnricher, ActionIdEnricher>();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.With<ActionIdEnricher>()
    .WriteTo.MSSqlServer(
        connectionString: connectionString,
        sinkOptions: new MSSqlServerSinkOptions
        {
            TableName = "Logs",
            AutoCreateSqlTable = false,
           
        },
        columnOptions: columnOptions
    )
    .CreateLogger();

builder.Host.UseSerilog();

builder.Host.UseSerilog();
var app = builder.Build();
app.UseCors("AllowAll");
app.UseRouting();
app.UseMiddleware<ActionIdMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();

    endpoints.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    endpoints.MapHealthChecksUI(options =>
    {
        options.UIPath = "/health-ui"; // Access via browser at /health-ui
    });
});

app.UseSerilogRequestLogging(); // logs HTTP requests

app.Run();
