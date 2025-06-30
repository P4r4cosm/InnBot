using InnBot;
using InnBot.Commands;
using InnBot.Services;
using Telegram.Bot;


var builder = Host.CreateApplicationBuilder(args);

var config = builder.Configuration;
builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(config["BotKey"]!));

builder.Services.AddScoped<UpdateHandlerService>();
builder.Services.AddScoped<CommandExecutor>();

//команды
builder.Services.AddScoped<ICommand, HelloCommand>();

builder.Services.AddHostedService<TelegramBotWorker>();



var host = builder.Build();
host.Run();