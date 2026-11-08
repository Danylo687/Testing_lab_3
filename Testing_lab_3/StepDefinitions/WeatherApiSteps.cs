using NUnit.Framework;
using Reqnroll;
using RestSharp;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lab3.StepDefinitions
{
    [Binding]
    public class WeatherApiSteps
    {
        private RestClient _client;
        private RestResponse _response;

        [Given(@"Я використовую відкрите API погоди ""(.*)""")]
        public void GivenIUsePublicWeatherApi(string url)
        {
            _client = new RestClient(url);
        }

        [When(@"Я відправляю GET запит для отримання погоди за координатами (.*) та (.*)")]
        public async Task WhenIRequestWeather(string lat, string lon)
        {
            var request = new RestRequest($"/forecast?latitude={lat}&longitude={lon}&current_weather=true", Method.Get);
            _response = await _client.ExecuteAsync(request);
        }

        [Then(@"Статус код відповіді має бути (.*)")]
        public void ThenStatusCodeShouldBe(int expectedStatusCode)
        {
            Assert.That((int)_response.StatusCode, Is.EqualTo(expectedStatusCode));
        }

        [Then(@"Відповідь має містити поточну температуру")]
        public void ThenResponseContainsTemperature()
        {
            var json = JsonDocument.Parse(_response.Content);

            // Дістаємо об'єкт current_weather, а з нього поле temperature
            var currentWeather = json.RootElement.GetProperty("current_weather");
            double temperature = currentWeather.GetProperty("temperature").GetDouble();

            // Виводимо інформацію про погоду у Львові
            Console.WriteLine($"\n--- ПОГОДА У ЛЬВОВІ ---\nПоточна температура: {temperature} °C\nПовний JSON: {_response.Content}\n");

            // Якщо значення успішно отримано, перевірка пройдена
            Assert.That(temperature, Is.Not.Null);
        }
    }
}