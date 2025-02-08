using Lemur.Server.Data;
using Lemur.Server.Models.ModelsTelegram;
using Lemur.Server.Services.ServicesTelegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LemurDbContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<TelegramBotOptions>(builder.Configuration.GetSection("TelegramBot"));
// Registrazione del servizio Telegram
builder.Services.AddSingleton<TelegramBotService>(sp =>
{
    var options = sp.GetRequiredService<IOptions<TelegramBotOptions>>().Value;
    var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
    return new TelegramBotService(options.BotToken, scopeFactory);
});

// Registrazione del Background Service per il bot
builder.Services.AddHostedService<TelegramBotBackgroundService>();

var app = builder.Build();

// Esegui l'automigrazione all'avvio
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LemurDbContext>();
    dbContext.Database.Migrate(); // Applica le migrazioni pendenti
}

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(builder => builder
    //.WithOrigins("https://localhost:5173")  // Limita le origini autorizzate
    .AllowAnyOrigin()
    .AllowAnyMethod()  // Consente tutti i metodi HTTP
    .AllowAnyHeader());  // Consente tutte le intestazioni

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
