namespace Lemur.Server.Services.ServicesTelegram;

public class TelegramBotBackgroundService : BackgroundService
{
    private readonly TelegramBotService _telegramBotService;

    public TelegramBotBackgroundService(TelegramBotService telegramBotService)
    {
        _telegramBotService = telegramBotService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Avvio del servizio Telegram Bot...");

        // Gestisco l'esecuzione dei task
        var periodicTask = _telegramBotService.StartPeriodicTaskAsync(stoppingToken);
        var handleUpdates = _telegramBotService.HandleUpdatesAsync();

        await Task.WhenAny(periodicTask, handleUpdates);

        stoppingToken.ThrowIfCancellationRequested();
    }
}
