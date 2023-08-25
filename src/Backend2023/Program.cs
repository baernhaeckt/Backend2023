using Backend2023.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

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
