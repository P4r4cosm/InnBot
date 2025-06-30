using InnBot.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace InnBot.Services;

public class UpdateHandlerService
{
    private readonly CommandExecutor _commandExecutor;
    private readonly ILogger<UpdateHandlerService> _logger;

    public UpdateHandlerService(CommandExecutor commandExecutor, ILogger<UpdateHandlerService> logger)
    {
        _commandExecutor = commandExecutor;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Интересуют только сообщения с текстом
        if (update.Message is not { Text: { } messageText })
            return;

        // Команда - это первое слово в сообщении
        var command = messageText.Split(' ')[0];

        try
        {
            await _commandExecutor.ExecuteCommandAsync(command, update, botClient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выполнении команды {Command}", command);
            // Тут же можно отправить пользователю сообщение об ошибке
            await botClient.SendMessage(update.Message.Chat.Id, "Произошла внутренняя ошибка.", cancellationToken: cancellationToken);
        }
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Ошибка при получении обновлений от Telegram");
        return Task.CompletedTask;
    }
}