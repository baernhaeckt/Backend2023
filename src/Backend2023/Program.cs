using Backend2023.Cognitive;
using Backend2023.Common;
using Backend2023.Hubs;
using Backend2023.Modules;
using Backend2023.OpenAI;
using Backend2023.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .Configure<ApplicationConfiguration>(builder.Configuration.GetRequiredSection(nameof(ApplicationConfiguration)))
    .AddScoped<SpeechServiceProvider>()
    .AddSingleton<IChatBot, ChatBot>()
    .AddSingleton<IConversations, Conversations>()
    .AddSingleton<IConversationSummarizer, ConversationSummarizer>();

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
        .WithOrigins("http://localhost:5173",
            "https://lively-island-07bd13b03.3.azurestaticapps.net")
        .AllowAnyHeader()
        .WithExposedHeaders("Authorization")
        .AllowCredentials());

app.UseRouting();
app.MapControllers();

app.UseEndpoints(endpoints => { endpoints.MapHub<AudioHub>("/audiohub"); });

app.Run();
