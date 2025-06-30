# Телеграм-бот для получения информации о компаниях по ИНН

## Функционал

Бот поддерживает следующие команды:

-   `/start` - Начало работы и приветствие.
-   `/help` - Справка по доступным командам.
-   `/hello` - Информация об авторе бота.
-   `/inn <ИНН1> <ИНН2> ...` - Получить наименование и адрес компаний по ИНН. Можно указать несколько ИНН, разделяя их пробелом, запятой или точкой с запятой.
-   `/last` - Повторить последнюю успешную команду (кроме `/start`, `/help`, `/last`).

## Технологический стек

-   **Платформа:** .NET 9
-   **Язык:** C#
-   **Взаимодействие с Telegram API:** `Telegram.Bot`
-   **Получение данных по ИНН:** API сервиса Dadata (используется `Dadata-client`)
-   **Тестирование:** `xUnit`, `NSubstitute`
-   **Развертывание:** `Docker`

## Скриншоты работы
![image](https://github.com/user-attachments/assets/8c2fa976-a3c8-4502-b4a6-1cc13ef3f5d3)
![image](https://github.com/user-attachments/assets/f613736d-63df-4785-8204-c29b10755b27)
![image](https://github.com/user-attachments/assets/1a2b84d5-eb1a-4a81-8f66-1553d1e7112b)


## Локальный запуск

### Требования
- .NET 9 SDK
- API-ключ от сервиса [Dadata](https://dadata.ru/)

### Инструкции
1.  **Клонируйте репозиторий:**
    ```bash
    git clone https://github.com/ВАШ_НИКНЕЙМ/ВАШ_РЕПОЗИТОРИЙ.git
    cd ВАШ_РЕПОЗИТОРИЙ
    ```

2.  **Настройте конфигурацию:**
    В папке `InnBot` создайте файл `appsettings.json` и заполните его по шаблону ниже:

    ```json
    {
      "BotKey": "ВАШ_ТЕЛЕГРАМ_БОТ_ТОКЕН_ЗДЕСЬ",
      "DadataConfiguration": {
        "ApiKey": "ВАШ_API_КЛЮЧ_ОТ_DADATA_ЗДЕСЬ"
      },
      "MyInfoConfiguration": {
        "Name": "Фамилия Имя",
        "Email": "ваш@email.com",
        "GitHubUrl": "https://github.com/ВАШ_НИКНЕЙМ",
        "ResumeUrl": "https://hh.ru/resume/ВАШ_ID"
      }
    }
    ```

3.  **Запустите приложение:**
    ```bash
    dotnet run --project InnBot/InnBot.csproj
    ```

## Развертывание с Docker

Проект содержит `Dockerfile` для сборки и запуска в изолированном контейнере.

1.  **Соберите Docker-образ:**
    ```bash
    docker build -t inn-bot .
    ```

2.  **Подготовьте `.env` файл** с переменными окружения:
    ```
    BotKey=ВАШ_ТЕЛЕГРАМ_БОТ_ТОКЕН_ЗДЕСЬ
    DadataConfiguration__ApiKey=ВАШ_API_КЛЮЧ_ОТ_DADATA_ЗДЕСЬ
    MyInfoConfiguration__Name="Фамилия Имя"
    MyInfoConfiguration__Email="ваш@email.com"
    MyInfoConfiguration__GitHubUrl="https://github.com/ВАШ_НИКНЕЙМ"
    MyInfoConfiguration__ResumeUrl="https://hh.ru/resume/ВАШ_ID"
    ```

3.  **Запустите контейнер:**
    ```bash
    docker run -d --restart always --env-file /путь/к/вашему/innbot.env --name my-inn-bot inn-bot
    ```
