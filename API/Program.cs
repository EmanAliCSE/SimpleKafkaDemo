using System;
using Confluent.Kafka;
using Domain.Data;
using Domain.Interfaces;
using Infrastructure.Services;
using KafkaWebApiDemo.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
         options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add Kafka services
builder.Services.AddSingleton<IProducer<Null, string>>(sp =>
    new ProducerBuilder<Null, string>(new ProducerConfig
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"]
    }).Build());


//builder.Services.AddSingleton<KafkaProducerService>();
builder.Services.AddHostedService<KafkaConsumerService>();
builder.Services.AddScoped<IOutboxService, OutboxService>();
builder.Services.AddHostedService<OutboxProcessorService>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

app.UseRouting();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
