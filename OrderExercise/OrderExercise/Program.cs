using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddSingleton<SomeService>();

var app = builder.Build();

app.UseMiddleware<CorrelationMiddleware>();

app.MapGet("/", () => "Hello World!");

app.Run();