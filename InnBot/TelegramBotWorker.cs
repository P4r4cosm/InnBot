// InnBot/TelegramBotWorker.cs
using InnBot.Services;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
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
        _logger.LogInformation("Бот {Username} ({BotId}) запущен.", me.Username, me.Id);
        
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // Получать все типы обновлений
        };
        
        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cancellationToken
        );
    }
    
    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Создаем новый scope для обработки этого конкретного обновления
        await using var scope = _serviceProvider.CreateAsyncScope();
        var updateHandler = scope.ServiceProvider.GetRequiredService<UpdateHandlerService>();

        try
        {
            await updateHandler.HandleUpdateAsync(botClient, update, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Произошла неперехваченная ошибка при обработке обновления {UpdateId}", update.Id);
        }
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Ошибка API Telegram: [{apiRequestException.ErrorCode}] {apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogError("Ошибка при получении обновлений (Polling): {ErrorMessage}", errorMessage);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Бот остановлен.");
        return Task.CompletedTask;
    }
}