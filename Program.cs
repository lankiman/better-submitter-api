using better_submitter_api;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();


var app = builder.Build();
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();

Services.Initialize(loggerFactory, app.Environment.ContentRootPath);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseHttpsRedirection();

app.Register();

app.Run();


