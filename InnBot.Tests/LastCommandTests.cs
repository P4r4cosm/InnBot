using InnBot.Commands;
using InnBot.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Telegram.Bot;
using Telegram.Bot.Requests; // Важный using
using Telegram.Bot.Types;

namespace InnBot.Tests;

public class LastCommandTests
{
    private readonly IUserHistoryService _historyService;
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<LastCommand> _logger;
    private readonly LastCommand _lastCommand;

    public LastCommandTests()
    {
        _historyService = Substitute.For<IUserHistoryService>();
        _botClient = Substitute.For<ITelegramBotClient>();
        _logger = Substitute.For<ILogger<LastCommand>>();
        _lastCommand = new LastCommand(_historyService, _logger);
    }

    private static Update CreateFakeUpdate(string text, long chatId = 123)
    {
        return new Update { Message = new Message { Chat = new Chat { Id = chatId }, Text = text } };
    }

    [Fact]
    public async Task ExecuteAsync_ShouldExecuteLastCommand_WhenHistoryIsNotEmpty()
    {
        // --- Arrange ---
        const long chatId = 123;
        
        // 1. Создаем мок "прошлой" команды и "прошлого" вызова
        var fakeLastCommand = Substitute.For<ICommand>();
        var fakeLastUpdate = CreateFakeUpdate("/inn 7707083893", chatId);

        // 2. Настраиваем сервис истории, чтобы он вернул наши моки
        _historyService.GetLastCommand(chatId).Returns((fakeLastCommand, fakeLastUpdate));

        // 3. Создаем текущий вызов команды /last
        var currentUpdate = CreateFakeUpdate("/last", chatId);

        // --- Act ---
        await _lastCommand.ExecuteAsync(currentUpdate, _botClient);

        // --- Assert ---
        // 1. САМАЯ ВАЖНАЯ ПРОВЕРКА: Убеждаемся, что метод ExecuteAsync был вызван на "прошлой" команде
        // с ее "прошлыми" данными.
        await fakeLastCommand.Received(1).ExecuteAsync(fakeLastUpdate, _botClient);

        // 2. Убеждаемся, что сама команда /last НЕ отправляла никаких сообщений.
        // Она только делегирует выполнение.
        await _botClient.DidNotReceive().SendRequest(Arg.Any<SendMessageRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotifyUser_WhenHistoryIsEmpty()
    {
        // --- Arrange ---
        const long chatId = 456;
        
        // 1. Настраиваем сервис истории так, чтобы он ничего не вернул
        _historyService.GetLastCommand(chatId).Returns((null, null));
        
        var currentUpdate = CreateFakeUpdate("/last", chatId);

        // 2. Настраиваем перехват запроса, чтобы проверить его содержимое
        SendMessageRequest? capturedRequest = null;
        _botClient.SendRequest(Arg.Do<SendMessageRequest>(req => capturedRequest = req), Arg.Any<CancellationToken>());

        // --- Act ---
        await _lastCommand.ExecuteAsync(currentUpdate, _botClient);

        // --- Assert ---
        // 1. Проверяем, что запрос на отправку сообщения был сделан
        await _botClient.Received(1).SendRequest(Arg.Any<SendMessageRequest>(), Arg.Any<CancellationToken>());

        // 2. Проверяем содержимое перехваченного запроса
        Assert.NotNull(capturedRequest);
        Assert.Equal(chatId, capturedRequest.ChatId);
        // Проверяем текст из реальной реализации команды
        Assert.Contains("Я не помню вашу последнюю команду", capturedRequest.Text);
    }
}