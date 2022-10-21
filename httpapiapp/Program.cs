using HttpApi;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSingleton<ITelemetryInitializer>(new RoleNameTelemetryInitializer("http-api"));

builder.Services.AddSingleton<MessageQueueClient>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/data", async (MessageQueueClient client) => Results.Text(await client.GetQueueInfo()));
app.MapPost("/data", async (MessageQueueClient client, [FromQuery]string message) =>
{
    var messageSent = await client.SendMessage(message);
    return messageSent ? 
        Results.Ok() : 
        new UnavailableResult();
});

app.Run();