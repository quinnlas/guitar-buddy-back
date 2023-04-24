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

app.MapPost("/tab/parse", ([FromBody] TabForm tabForm) =>
{
  var song = new Song(tabForm);

  return JsonSerializer.Serialize(song);
});

app.MapPost("/tab/optimize", ([FromBody] OptimizeForm optimizeForm) =>
{
  var playing = new Playing(optimizeForm);

  playing.optimizeDistance(optimizeForm.iterations);

  return playing.ToString();
});

app.Run();
