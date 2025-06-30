using InnBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace InnBot;

public class TelegramBotWorker : IHostedService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TelegramBotWorker> _logger;
    public TelegramBotWorker(ITelegramBotClient botClient, IServiceProvider serviceProvider, ILogger<TelegramBotWorker> logger)
    {
        _botClient = botClient;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var me = await _botClient.GetMe(cancellationToken);
        _logger.LogInformation("Бот {Username} запущен.", me.Username);
        
        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandlePollingErrorAsync,
            //pollingErrorHandler: _updateHandler.HandlePollingErrorAsync,
            receiverOptions: new() { AllowedUpdates = Array.Empty<UpdateType>() }, // Получать все типы обновлений
            cancellationToken: cancellationToken
        );
    }
    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Создаем новый scope для обработки этого конкретного обновления
        await using var scope = _serviceProvider.CreateAsyncScope();
        
        // Внутри этого scope получаем наш Scoped сервис
        var updateHandler = scope.ServiceProvider.GetRequiredService<UpdateHandlerService>();

        // Вызываем его метод
        await updateHandler.HandleUpdateAsync(botClient, update, cancellationToken);
    }
    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Ошибка при получении обновлений от Telegram.");
        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Бот остановлен.");
        return Task.CompletedTask;
    }
}