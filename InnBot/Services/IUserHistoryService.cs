using InnBot.Commands;
using Telegram.Bot.Types;

namespace InnBot.Services;

public interface IUserHistoryService
{
    // Сохраняем последнюю команду для пользователя
    void SetLastCommand(long chatId, ICommand command, Update update);
    
    // Получаем последнюю команду для пользователя
    (ICommand? Command, Update? Update) GetLastCommand(long chatId);
}