using Telegram.Bot;
using Telegram.Bot.Types;

namespace InnBot.Commands;

public interface ICommand
{
    string Name { get; } // Например, "/start"
    Task ExecuteAsync(Update update, ITelegramBotClient botClient);
}