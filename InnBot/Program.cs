using InnBot;
using InnBot.Commands;
using InnBot.Configuration;
using InnBot.DataProviders;
using InnBot.Services;
using Telegram.Bot;


var builder = Host.CreateApplicationBuilder(args);
//builder.Logging.AddConsole();

var config = builder.Configuration;
builder.Services.Configure<MyInfoConfiguration>(config.GetSection("MyInfoConfiguration"));
builder.Services.Configure<DadataConfiguration>(config.GetSection("DadataConfiguration"));

builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(config["BotKey"]!));
builder.Services.AddSingleton<IDataProvider, DadataProvider>();
builder.Services.AddSingleton<IUserHistoryService, InMemoryUserHistoryService>();


builder.Services.AddScoped<UpdateHandlerService>();
builder.Services.AddScoped<CommandExecutor>();

//команды
builder.Services.AddScoped<ICommand, HelloCommand>();
builder.Services.AddScoped<ICommand, InnCommand>();
builder.Services.AddScoped<ICommand, HelpCommand>();
builder.Services.AddScoped<ICommand, LastCommand>();
builder.Services.AddScoped<ICommand, StartCommand>();

builder.Services.AddHostedService<TelegramBotWorker>();



var host = builder.Build();
host.Run();