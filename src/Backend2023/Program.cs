using Backend2023.Cognitive;
using Backend2023.Hubs;
using Backend2023.Modules;

var builder = WebApplication.CreateBuilder(args);

AzureConfiguration? configuration = builder.Configuration.GetSection(AzureConfiguration.Path).Get<AzureConfiguration>();
builder.Services
    .AddSingleton(configuration!)
    .AddScoped<SpeechServiceProvider>()
    .AddScoped<ChatBot>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR(opt =>
{
    opt.EnableDetailedErrors = true;
    opt.MaximumReceiveMessageSize = 1024 * 1024 * 512; // 512kb
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(x =>
    x.AllowAnyMethod()
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .WithExposedHeaders("Authorization")
        .AllowCredentials());

app.UseRouting();
app.MapControllers();

app.UseEndpoints(endpoints => { endpoints.MapHub<AudioHub>("/audiohub"); });

app.Run();
