using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Configuration;
using Telegram.Bot.Types.ReplyMarkups;
using CourseDataManager.Bot.Models;
using System.Text.RegularExpressions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Telegram.Bot.Types.InputFiles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Newtonsoft.Json.Linq;

namespace CourseDataManager.Bot
{
    internal class CourseDataManagerBot
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ReceiverOptions _receiverOptions;
        private readonly CancellationTokenSource _cts;
        private readonly ApiClient _apiClient;
        private readonly string _patternLogin;
        private readonly string _patternRegister;
        private readonly string _patternFindDoc;
        private readonly string _patternLink;
        private readonly IHostingEnvironment _hostingEnvironment;

        public CourseDataManagerBot()
        {
            _botClient = new TelegramBotClient(ConfigurationManager.AppSettings.Get("BotToken"));
            _cts = new CancellationTokenSource();
            _receiverOptions = new() { AllowedUpdates = Array.Empty<UpdateType>() };
            _apiClient = new ApiClient();
            _patternLogin = ConfigurationManager.AppSettings.Get("PatternLogin");
            _patternRegister = ConfigurationManager.AppSettings.Get("PatternRegister");
            _patternFindDoc = ConfigurationManager.AppSettings.Get("PatternFindDoc");
            _patternLink = ConfigurationManager.AppSettings.Get("PatternLink");
            _hostingEnvironment = new HostingEnvironment();
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
            if (update.Type == UpdateType.Message && update?.Message?.Document != null)
                await HandleMessageDocumentAsync(botClient, update.Message);
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
            // Start message
            if (message.Text == "/start")
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Привіт! Я ZNO History Bot. І я допоможу тобі розібратися та знайти усі матеріали курсу");
                await client.SendTextMessageAsync(message.Chat.Id, "Якщо ти уже зареєстрований(на), то давай продовжимо. Увійди у систему.");
                await client.SendTextMessageAsync(message.Chat.Id, "Щоб продовжити натисни \"Увійти\"", replyMarkup: replyKeyboardMarkup);
                return;
            }
            // Login message
            else if (message.Text.ToLower() == "увійти" || message.Text.ToLower() == "/login")
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

                var user = ParseForLogin(message.Text);
                user.ChatId = message.Chat.Id;
                var result = await _apiClient.Login(user);
                await client.SendTextMessageAsync(message.Chat.Id, result);
                return;
            }
            // Log out
            else if (message.Text.ToLower() == "вийти" || message.Text.ToLower() == "/logout")
            {
                var token = await CheckToken(client, message);
                if (token == null)
                    return;

                await _apiClient.UpdateJwtToken(message.Chat.Id, string.Empty);
                await client.SendTextMessageAsync(message.Chat.Id, "Ви успішно вийшли.)");
                return;
            }
            //registration message
            else if (message.Text.ToLower() == "/register")
            {
                var token = await CheckToken(client, message);
                if (token == null)
                    return;

                if (IsAdmin(token))
                    await client.SendTextMessageAsync(message.Chat.Id, "Для реєстрації користувача уведіть наступну команду:" +
                        "\n\nРеєстрація\nІм'я: ****\nПрізвище: ****\nEmail: ****\nПароль: ****");
                else
                    await client.SendTextMessageAsync(message.Chat.Id, "Реєструвати користувачів може тільки адмін.");
                return;
            }
            //registration
            else if (Regex.IsMatch(message.Text.Trim(), _patternRegister, RegexOptions.IgnoreCase))
            {
                var token = await CheckToken(client, message);
                if (token == null)
                    return;

                if (IsAdmin(token))
                {
                    var user = ParseForRegister(message.Text);
                    var result = await _apiClient.RegisterStudent(user, token);
                    await client.SendTextMessageAsync(message.Chat.Id, result);
                }
                else
                    await client.SendTextMessageAsync(message.Chat.Id, "Реєструвати користувачів може тільки адмін.");
                return;
            }
            //find document message
            else if (message.Text.ToLower() == "Матеріали" || message.Text.ToLower() == "/finddoc")
            {
                var token = await CheckToken(client, message);
                if (token == null)
                    return;

                await client.SendTextMessageAsync(message.Chat.Id, "Для пошуку матеріалів введіть таку команду (Тема: назва теми)");
                return;
            }
            //find document
            else if (Regex.IsMatch(message.Text.Trim(), _patternFindDoc, RegexOptions.IgnoreCase))
            {
                var token = await CheckToken(client, message);
                if (token == null)
                    return;

                var path = _hostingEnvironment.WebRootPath + "/Files/";
                string[] files = Directory.GetFiles(path);
                string theme = message.Text.Split(":")[1].Trim();

                foreach (string file in files)
                {
                    if (file.Contains(theme, StringComparison.CurrentCultureIgnoreCase))
                    {
                        await using Stream stream = System.IO.File.OpenRead(file);
                        await client.SendDocumentAsync(
                            chatId: message.Chat.Id,
                            document: new InputOnlineFile(content: stream, fileName: file));
                    }
                }

                var links = await _apiClient.GetLinksByName(theme, token);
                if (links != null)
                {
                    foreach (var link in links)
                    {
                        await client.SendTextMessageAsync(message.Chat.Id, $"Назва: {link.Name}\n{link.Link_}");
                    }
                }
                await client.SendTextMessageAsync(message.Chat.Id, $"Це все що знайдемо за заданою темою ({theme})");
                return;
            }
            // create link message
            else if (message.Text == "/savelink")
            {
                var token = await CheckToken(client, message);
                if (token == null)
                    return;

                if (IsAdmin(token))
                    await client.SendTextMessageAsync(message.Chat.Id, $"Щоб зберегти посилання на матеріали скористайся наступною командою\n" +
                    $"Назва: ****\n" +
                    $"Посилання: ****)");
                else
                    await client.SendTextMessageAsync(message.Chat.Id, "Зберігати посилання може тільки адмін.");
                return;;
            }
            // create link material
            else if (Regex.IsMatch(message.Text.Trim(), _patternLink, RegexOptions.IgnoreCase))
            {
                var token = await CheckToken(client, message);
                if (token == null)
                    return;

                var link = ParseLink(message.Text);

                var result = await _apiClient.CreateLink(link, token);
                await client.SendTextMessageAsync(message.Chat.Id, result);
            }
            //help
            else if (message.Text.ToLower() == "/help")
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Ось список усіх команд:\n" +
                    "/start - розпочати\n" +
                    "/login - увійти в систему\n" +
                    "Для входу використовуйте наступний шаблон:\n   логін: **** пароль: ****\n" +
                    "/logout - вийти з системи\n" +
                    "/finddoc - пошук матеріалів\n" +
                    "Для пошуку матеріалів користуйтеся настпним шаблоном:\n   Тема: ****");
            }
            // Anothe message
            else
            {
                await client.SendTextMessageAsync(
                    message.Chat.Id,
                    "Невірна команду. Використай /help, щоб побачити можливі команди"
                );
            }
        }

        private async Task HandleMessageDocumentAsync(ITelegramBotClient client, Message message)
        {
            var token = await CheckToken(client, message);
            if (token == null)
                return;

            if (message.Document != null)
            {
                string destinationFilePath = _hostingEnvironment.WebRootPath + "/Files/" + message.Document.FileName;
                await using Stream fileStream = System.IO.File.OpenWrite(destinationFilePath);

                var file = await client.GetInfoAndDownloadFileAsync(
                    fileId: message.Document.FileId,
                    destination: fileStream,
                    cancellationToken: _cts.Token);
                await client.SendTextMessageAsync(message.Chat.Id, "Файл успішно збережено");
                return;
            }
            {
                await client.SendTextMessageAsync(
                    message.Chat.Id,
                    "Невірна команду. Використай /help, щоб побачити можливі команди"
                );
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

        #region Help message
        private async Task<string> CheckToken(ITelegramBotClient client, Message message)
        {
            var token = await _apiClient.GetJwtToken(message.Chat.Id);
            if (string.IsNullOrEmpty(token))
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Спочатку потрібно увійти)");
                return null;
            }
            else
                return token;
        }
        #endregion

        #region Parse model
        private UserLogin ParseForLogin(string message)
        {
            var logPass = message.Split(" ");
            var user = new UserLogin
            {
                Email = logPass[1].Trim(),
                Password = logPass[3].Trim()
            };
            return user;
        }

        private UserRegister ParseForRegister(string message)
        {
            var registerInfo = message.Split(":");
            var user = new UserRegister
            {
                UserName = registerInfo[1].Trim().Split("\n")[0],
                UserSurname = registerInfo[2].Trim().Split("\n")[0],
                Email = registerInfo[3].Trim().Split("\n")[0],
                Password = registerInfo[4].Trim().Split("\n")[0]
            };
            return user;
        }

        private Link ParseLink(string message)
        {
            var linkInfo = message.Split(":");
            var link = new Link
            {
                Name = linkInfo[1].Trim().Split("\n")[0],
                Link_ = linkInfo[3].Trim().Split("\n")[0],
            };
            return link;
        }
        #endregion

        private bool IsAdmin(string tokenString)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(tokenString);
            var role = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            return role == "Admin";
        }
    }
}
