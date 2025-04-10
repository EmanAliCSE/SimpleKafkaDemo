using System;
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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddServiceExtensions(builder.Configuration);
builder.Services.AddConsumerServices(builder.Configuration);


//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();

var app = builder.Build();

app.UseRouting();
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

app.Run();
