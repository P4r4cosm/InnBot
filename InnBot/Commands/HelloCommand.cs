using System.Text;
using InnBot.Configuration;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace InnBot.Commands;

public class HelloCommand: ICommand
{
    public string Name { get; } = "/hello";
    private readonly IOptions<MyInfoConfiguration> _myInfo;
    private readonly ILogger<HelloCommand> _logger;
    public HelloCommand(IOptions<MyInfoConfiguration> myInfo, ILogger<HelloCommand> logger)
    {
        _myInfo = myInfo;
        _logger = logger;
    }
    public async Task ExecuteAsync(Update update, ITelegramBotClient botClient)
    {
        var chatId = update.Message!.Chat.Id;
        var info = _myInfo.Value; // .Value дает нам доступ к самому объекту конфигурации

        // Проверяем, что данные были загружены из конфигурации
        if (string.IsNullOrWhiteSpace(info.Name))
        {
            await botClient.SendMessage(
                chatId: chatId,
                text: "Извините, информация обо мне не настроена."
            );
            return;
        }

        // Используем StringBuilder для красивого формирования многострочного сообщения
        var messageBuilder = new StringBuilder();
        messageBuilder.AppendLine($"**Привет! Вот информация об авторе бота:**");
        messageBuilder.AppendLine(); // Пустая строка для отступа
        messageBuilder.AppendLine($"👤 **Имя:** {info.Name}");
        messageBuilder.AppendLine($"📧 **Email:** {info.Email}");
        messageBuilder.AppendLine($"🐙 **GitHub:** {info.GitHubUrl}");
        messageBuilder.AppendLine($"📄 **Резюме на hh.ru:** {info.ResumeUrl}");
        
        await botClient.SendMessage(
            chatId: chatId,
            text: messageBuilder.ToString(),
            // Включаем Markdown, чтобы сделать текст жирным
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown
        );
    }
}