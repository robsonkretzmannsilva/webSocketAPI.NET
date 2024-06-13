using WebSocketPool.Domain.Interface;
using Azure.Messaging.WebPubSub;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<IWebSocketPool, WebSocketPool.Service.Service.WebSocketPool>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseWebSockets();
app.UseAuthorization();


app.MapControllers();

app.Run();

