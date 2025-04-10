using System;
using Confluent.Kafka;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Services;
using KafkaWebApiDemo.Services;
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
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
