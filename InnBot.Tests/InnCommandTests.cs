using InnBot.Commands;
using InnBot.DataProviders;
using InnBot.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Telegram.Bot;
using Telegram.Bot.Requests; // Важный using для SendMessageRequest
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace InnBot.Tests;

public class InnCommandTests
{
    private readonly IDataProvider _dataProvider;
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<InnCommand> _logger;
    private readonly InnCommand _innCommand;

    public InnCommandTests()
    {
        _dataProvider = Substitute.For<IDataProvider>();
        _botClient = Substitute.For<ITelegramBotClient>();
        _logger = Substitute.For<ILogger<InnCommand>>();
        _innCommand = new InnCommand(_dataProvider, _logger);
    }

    private static Update CreateFakeUpdate(string text, long chatId = 123)
    {
        return new Update { Message = new Message { Chat = new Chat { Id = chatId }, Text = text } };
    }
    
    [Fact]
    public async Task ExecuteAsync_ShouldProduceDetailedReport_ForMixedInput()
    {
        // Arrange
        const string validFoundInn = "7707083893";
        const string validNotFoundInn = "1234567890";
        const string invalidLengthInn = "12345";
        
        var fakeUpdate = CreateFakeUpdate($"/inn {validFoundInn} {validNotFoundInn} {invalidLengthInn}");
        _dataProvider.GetCompanyInfoByInnAsync(Arg.Any<IEnumerable<string>>())
            .Returns(new List<CompanyInfo> { new() { Inn = validFoundInn, Name = "ПАО СБЕРБАНК" } });

        // Act
        await _innCommand.ExecuteAsync(fakeUpdate, _botClient);
        
        // Assert
        // ПРОВЕРЯЕМ SendRequest, ИНСПЕКТИРУЯ СВОЙСТВА SendMessageRequest
        await _botClient.Received(1).SendRequest(
            Arg.Is<SendMessageRequest>(req =>
                // 1. Проверяем Id чата
                req.ChatId == fakeUpdate.Message.Chat.Id &&
                // 2. Проверяем, что текст содержит все три обязательных блока
                req.Text.Contains("✅ **Найденные компании:**") &&
                req.Text.Contains("🟡 **Не найдены в базе:**") &&
                req.Text.Contains("❌ **Некорректный формат:**") &&
                // 3. Проверяем, что режим парсинга - Markdown
                req.ParseMode == ParseMode.Markdown
            ),
            Arg.Any<CancellationToken>()
        );
    }

    [Theory]
    [InlineData("/inn")]
    [InlineData("/inn ")]
    public async Task ExecuteAsync_ShouldSendHelpMessage_WhenNoInnsAreProvided(string messageText)
    {
        // Arrange
        var fakeUpdate = CreateFakeUpdate(messageText);
        
        // Act
        await _innCommand.ExecuteAsync(fakeUpdate, _botClient);

        // Assert
        await _dataProvider.DidNotReceive().GetCompanyInfoByInnAsync(Arg.Any<IEnumerable<string>>());
        
        // ПРОВЕРЯЕМ SendRequest с сообщением-инструкцией
        await _botClient.Received(1).SendRequest(
            Arg.Is<SendMessageRequest>(req =>
                req.ChatId == fakeUpdate.Message.Chat.Id &&
                req.Text.Contains("Пожалуйста, укажите один или несколько ИНН") &&
                req.ParseMode == ParseMode.Markdown
            ),
            Arg.Any<CancellationToken>()
        );
    }
    
    [Fact]
    public async Task ExecuteAsync_ShouldNotifyUserAndLog_WhenDataProviderThrowsException()
    {
        // Arrange
        var fakeUpdate = CreateFakeUpdate("/inn 7707083893");
        var apiException = new Exception("API is down");
        _dataProvider.GetCompanyInfoByInnAsync(Arg.Any<IEnumerable<string>>())
            .ThrowsAsync(apiException);

        SendMessageRequest? capturedRequest = null;
        _botClient.SendRequest(Arg.Do<SendMessageRequest>(req => capturedRequest = req), Arg.Any<CancellationToken>());
        
        // Act
        await _innCommand.ExecuteAsync(fakeUpdate, _botClient);

        // Assert
        // Проверяем, что запрос был отправлен
        await _botClient.Received(1).SendRequest(Arg.Any<SendMessageRequest>(), Arg.Any<CancellationToken>());
    
        // Проверяем перехваченный объект
        Assert.NotNull(capturedRequest);
        Assert.Equal(fakeUpdate.Message.Chat.Id, capturedRequest.ChatId);
        Assert.Contains("Произошла ошибка при получении данных от внешнего сервиса. Попробуйте позже.", capturedRequest.Text);
    
        //Проверяем, что ParseMode имеет значение по умолчанию для enum - ParseMode.None
        Assert.Equal(ParseMode.None, capturedRequest.ParseMode);

        // Проверка логирования
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString().Contains("Ошибка при получении данных от провайдера")),
            apiException,
            Arg.Any<Func<object, Exception, string>>());
    }
}