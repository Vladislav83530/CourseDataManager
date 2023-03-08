using CourseDataManager.Bot.Models;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Telegram.Bot.Types;

namespace CourseDataManager.Bot
{
    internal class ApiClient
    {
        private readonly HttpClient _client;
        private static string _address;

        public ApiClient()
        {
            _client = new HttpClient();
            _address = ConfigurationManager.AppSettings.Get("ApiAddress");
        }

        public async Task<string> Login(UserLogin user)
        {
            var response = await _client.PostAsJsonAsync(_address + $"/api/Auth/login", user);
            if (response.IsSuccessStatusCode)
                return "Ви успішно увійшли.";
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
        }

        public async Task<string> GetJwtToken(long chatId)
        {
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(_address + $"/api/Auth/token?chatId={chatId}");
            var response = await _client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        public async Task<string> UpdateJwtToken(long chatId, string jwtToken)
        {         
            var response = await _client.PostAsync(_address + $"/api/Auth/updatetoken?chatId={chatId}&jwtToken={jwtToken}", null );
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        public async Task<string> RegisterStudent(UserRegister user, string token)
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"bearer {token}");
            var response = await _client.PostAsJsonAsync(_address + $"/api/Auth/register", user);
            if (response.IsSuccessStatusCode)
                return "Користувач успішно зареєстрований";
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return "Тільки адмін може реєструвати користувачів";
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
        }
    }
}
