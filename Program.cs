using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowAnyOrigin", builder =>
  {
    builder
      .AllowAnyOrigin()
      .AllowAnyMethod()
      .AllowAnyHeader();
  });
});

var app = builder.Build();

app.UseCors("AllowAnyOrigin");

app.MapGet("/", () => "Hello World!");

app.MapPost("/tab/parse", ([FromBody] string tabText) =>
{
  Song song = new Song(tabText);
  return JsonSerializer.Serialize(song);
});



app.Run();
