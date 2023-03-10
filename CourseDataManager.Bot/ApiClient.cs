using CourseDataManager.Bot.Models;
using Newtonsoft.Json;
using System.Configuration;
using System.Net.Http.Json;

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
            _client.DefaultRequestHeaders.Remove("Authorization");

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

        public async Task<IEnumerable<Link>> GetLinksByName(string name, string token)
        {
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(_address + $"/api/Link/links?linkName={name}");

            _client.DefaultRequestHeaders.Add("Authorization", $"bearer {token}");
            var response = await _client.SendAsync(request);
            _client.DefaultRequestHeaders.Remove("Authorization");

            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<List<Link>>(content);
            return result;
        }

        public async Task<string> CreateLink(Link link, string token)
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"bearer {token}");
            var response = await _client.PostAsJsonAsync(_address + $"/api/Link/create", link);
            _client.DefaultRequestHeaders.Remove("Authorization");

            if (response.IsSuccessStatusCode)
                return "Посилання успішо збережено";
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
               return "Тільки aдмін може зберігати посилання";
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                throw new Exception(result);
            }
        }

        public async Task<IEnumerable<Student>> GetStudents(string token)
        {
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(_address + $"/api/Student/all");

            _client.DefaultRequestHeaders.Add("Authorization", $"bearer {token}");
            var response = await _client.SendAsync(request);
            _client.DefaultRequestHeaders.Remove("Authorization");

            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<List<Student>>(content);
            return result;
        }

        public async Task<string> ReverseIsAvailableValue(string email, string isAvailable, string token)
        {
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(_address + $"/api/Student/isAvailable/?email={email}&isAvailable={isAvailable}");

            _client.DefaultRequestHeaders.Add("Authorization", $"bearer {token}");
            var response = await _client.SendAsync(request);
            _client.DefaultRequestHeaders.Remove("Authorization");

            if (response.IsSuccessStatusCode)
                return "Успішно";
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return "Тільки aдмін може змінювати підписку";
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                throw new Exception(result);
            }
        }
    }
}
