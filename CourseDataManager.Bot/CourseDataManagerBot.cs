using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Configuration;
using Telegram.Bot.Types.ReplyMarkups;
using CourseDataManager.Bot.Models;
using System.Text.RegularExpressions;

namespace CourseDataManager.Bot
{
    internal class CourseDataManagerBot
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ReceiverOptions _receiverOptions;
        private readonly CancellationTokenSource _cts;
        private readonly ApiClient _apiClient;
        private readonly string _patternLogin;

        public CourseDataManagerBot()
        {
            _botClient = new TelegramBotClient(ConfigurationManager.AppSettings.Get("BotToken"));
            _cts = new CancellationTokenSource();
            _receiverOptions = new() { AllowedUpdates = Array.Empty<UpdateType>() };
            _apiClient = new ApiClient();
            _patternLogin = ConfigurationManager.AppSettings.Get("PatternLogin");
        }

        public async Task Start()
        {
            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: _receiverOptions,
                cancellationToken: _cts.Token);

            var me = await _botClient.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            _cts.Cancel();
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
                await HandleMessageAsync(botClient, update.Message);
        }

        private async Task HandleMessageAsync(ITelegramBotClient client, Message message)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(
                    new[]
                    {
                        new KeyboardButton[] { "Увійти", "Вийти" }
                    }
                )
            {
                ResizeKeyboard = true
            };
            //await Console.Out.WriteLineAsync($"User: {message.Chat.FirstName} {message.Chat.LastName}. Message: {message.Text}. ChatId: {message.Chat.Id}");
            // Start message
            if (message.Text == "/start")
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Привіт! Я ZNO History Bot. І я допоможу тобі розібратися та знайти усі матеріали курсу");
                await client.SendTextMessageAsync(message.Chat.Id, "Якщо ти уже зареєстрований(на), то давай продовжимо. Увійди у систему.");          
                await client.SendTextMessageAsync(message.Chat.Id, "Щоб продовжити натисни \"Увійти\"", replyMarkup: replyKeyboardMarkup);
                return;
            }
            // Login message
            else if (message.Text == "Увійти")
            {
                if (!string.IsNullOrEmpty(await _apiClient.GetJwtToken(message.Chat.Id)))
                {
                    await client.SendTextMessageAsync(message.Chat.Id, "Спочатку потрібно вийти)");
                    return;
                }

                await client.SendTextMessageAsync(message.Chat.Id, "Для входу введи логін та пароль через пробіл (логін: **** пароль: ****)");
                return;
            }
            // Login
            else if (Regex.IsMatch(message.Text.Trim(), _patternLogin, RegexOptions.IgnoreCase))
            {
                if (!string.IsNullOrEmpty(await _apiClient.GetJwtToken(message.Chat.Id)))
                {             
                    await client.SendTextMessageAsync(message.Chat.Id, "Спочатку потрібно вийти)");
                    return;
                }

                var user = ParseLoginAndPassword(message.Text);
                user.ChatId = message.Chat.Id;
                var result = await _apiClient.Login(user);
                await client.SendTextMessageAsync(message.Chat.Id, result);
                return;
            }
            // Log out
            else if (message.Text == "Вийти")
            {
                if (string.IsNullOrEmpty(await _apiClient.GetJwtToken(message.Chat.Id)))
                {
                    await client.SendTextMessageAsync(message.Chat.Id, "Спочатку потрібно увійти)");
                    return;
                }

                await _apiClient.UpdateJwtToken(message.Chat.Id, string.Empty);
                await client.SendTextMessageAsync(message.Chat.Id, "Ви успішно вийшли.)");
                return;
            }
            //else if (message.Text == "show")
            //{
            //    if (string.IsNullOrEmpty(await _apiClient.GetJwtToken(message.Chat.Id)))
            //    {
            //        await client.SendTextMessageAsync(message.Chat.Id, "null");
            //        return;
            //    }
            //    await client.SendTextMessageAsync(message.Chat.Id, await _apiClient.GetJwtToken(message.Chat.Id));
            //    return;
            //}
            // Anothe message
            else
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Невірна команда спробуй ще раз");
            }
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private UserLogin ParseLoginAndPassword(string message)
        {
            var logPass = message.Split(" ");
            UserLogin user = new();
            user.Email = logPass[1].Trim();
            user.Password = logPass[3].Trim();
            return user;
        }
    }
}
