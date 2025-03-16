using Lemur.Server.Data;
using Lemur.Server.Models.ModelsTelegram;
using Lemur.Server.Services.ServicesTelegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Data.Common;
using System.Net;
using System.Net.Http;


var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    // Ambiente di sviluppo
    builder.Services.AddDbContext<LemurDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DevConnection")));
}
else
{
    // Ambiente di rilascio (o qualsiasi altro ambiente non di sviluppo)
    builder.Services.AddDbContext<LemurDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ??
                             throw new InvalidOperationException("DefaultConnection stringa di connessione non trovata.")));
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<TelegramBotOptions>(builder.Configuration.GetSection("TelegramBot"));


// Registrazione del servizio Telegram
// Configura HttpClient con il proxy
//builder.Services.AddHttpClient("TelegramBotClient")
//    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
//    {
//        Proxy = new WebProxy("http://winproxy.server.lan:3128", true)
//        {
//            UseDefaultCredentials = false,
//        },
//        UseProxy = true,
//    });


if (builder.Environment.IsDevelopment())
{
    // Registrazione del servizio Telegram
    builder.Services.AddSingleton<TelegramBotService>(sp =>
    {
        var options = sp.GetRequiredService<IOptions<TelegramBotOptions>>().Value;
        var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
        //var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
        //var httpClient = httpClientFactory.CreateClient("TelegramBotClient");

        return new TelegramBotService(options.BotToken, scopeFactory);
    });

}
else
{
    // Registrazione del servizio Telegram
    builder.Services.AddSingleton<TelegramBotService>(sp =>
    {
        var options = sp.GetRequiredService<IOptions<TelegramBotOptions>>().Value;
        var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
        //var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

        return new TelegramBotService(options.BotToken, scopeFactory, true);
    });
}



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
