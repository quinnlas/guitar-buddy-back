var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
  options.AddPolicy(
    name: MyAllowSpecificOrigins,
    policy => {
      policy.WithOrigins("http://localhost:5173"); // TODO config
    });
});

var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);

app.MapGet("/", () => "Hello World!");

app.Run();
