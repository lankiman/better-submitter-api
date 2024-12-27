using System.Text.Json;
using System.Text.Json.Serialization;
using better_submitter_api;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddCors();

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});



var app = builder.Build();
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();


Services.Initialize(loggerFactory, app.Environment.ContentRootPath);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseCors(policy =>
{
    policy.AllowAnyOrigin()  // Allow any origin
        .AllowAnyHeader()  // Allow any HTTP headers
        .AllowAnyMethod(); // Allow any HTTP methods
});

app.Register();

app.Run();



