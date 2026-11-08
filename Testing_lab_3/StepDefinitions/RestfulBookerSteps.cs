using NUnit.Framework;
using Reqnroll;
using RestSharp;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lab3.StepDefinitions
{
    [Binding]
    public class RestfulBookerSteps
    {
        private RestClient _client;
        private RestResponse _response;
        private string _token;
        private string _bookingId;

        [Given(@"Я підключаюсь до API ""(.*)""")]
        public void GivenIConnectToApi(string url)
        {
            _client = new RestClient(url);
        }

        [Given(@"Я отримую токен авторизації з логіном ""(.*)"" та паролем ""(.*)""")]
        public async Task GivenIGetAuthToken(string username, string password)
        {
            var request = new RestRequest("/auth", Method.Post);
            request.AddJsonBody(new { username, password });

            _response = await _client.ExecuteAsync(request);
            var json = JsonDocument.Parse(_response.Content);
            _token = json.RootElement.GetProperty("token").GetString();
        }

        [When(@"Я створюю нове бронювання для ""(.*)"" ""(.*)""")]
        public async Task WhenICreateBooking(string firstName, string lastName)
        {
            var request = new RestRequest("/booking", Method.Post);

            // Повідомляємо серверу, що очікуємо відповідь у форматі JSON
            request.AddHeader("Accept", "application/json");

            request.AddJsonBody(new
            {
                firstname = firstName,
                lastname = lastName,
                totalprice = 150,
                depositpaid = true,
                bookingdates = new { checkin = "2026-05-15", checkout = "2026-05-20" },
                additionalneeds = "Breakfast"
            });

            _response = await _client.ExecuteAsync(request);
            var json = JsonDocument.Parse(_response.Content);
            _bookingId = json.RootElement.GetProperty("bookingid").ToString();

            // Виводимо інформацію про створене бронювання для наочності
            Console.WriteLine($"\n--- СТВОРЕНО БРОНЮВАННЯ (ID: {_bookingId}) ---\n{_response.Content}\n");
        }

        [Then(@"Бронювання має бути успішно створене")]
        public void ThenBookingIsCreated()
        {
            Assert.That((int)_response.StatusCode, Is.EqualTo(200));
            Assert.That(_bookingId, Is.Not.Null.And.Not.Empty);
        }

        [When(@"Я запитую дані створеного бронювання")]
        public async Task WhenIRequestBookingData()
        {
            var request = new RestRequest($"/booking/{_bookingId}", Method.Get);
            // Формат API вимагає передавати Accept заголовок для GET запиту
            request.AddHeader("Accept", "application/json");
            _response = await _client.ExecuteAsync(request);
        }

        [Then(@"Відповідь має містити ім'я ""(.*)""")]
        public void ThenResponseContainsName(string expectedName)
        {
            // Виводимо отримані дані з сервера для наочності
            Console.WriteLine($"\n--- ОТРИМАНІ ДАНІ З СЕРВЕРА ---\n{_response.Content}\n");

            var json = JsonDocument.Parse(_response.Content);
            string actualName = json.RootElement.GetProperty("firstname").GetString();
            Assert.That(actualName, Is.EqualTo(expectedName));
        }

        [When(@"Я видаляю це бронювання")]
        public async Task WhenIDeleteBooking()
        {
            var request = new RestRequest($"/booking/{_bookingId}", Method.Delete);
            // Restful-booker вимагає токен у заголовку Cookie для видалення
            request.AddHeader("Cookie", $"token={_token}");
            _response = await _client.ExecuteAsync(request);
        }

        [Then(@"Бронювання має бути успішно видалене")]
        public void ThenBookingIsDeleted()
        {
            // API повертає 201 Created при успішному видаленні
            Assert.That((int)_response.StatusCode, Is.EqualTo(201));
        }
    }
}