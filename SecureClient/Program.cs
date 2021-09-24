using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace SecureClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Making the call............");
            RunAsync().GetAwaiter().GetResult();
        }

        private static async Task RunAsync()
        {
            AuthConfig config = AuthConfig.ReadFromJsonFile("appsettings.json");
            Console.WriteLine($"{config.Authority}");
            IConfidentialClientApplication app;

            app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                    .WithClientSecret(config.ClientSecret)
                    .WithAuthority(new Uri(config.Authority))
                    .Build();

            string[] resourceIds = new string[] { config.ResourceId };

            AuthenticationResult result = null;
            try
            {
                result = await app.AcquireTokenForClient(resourceIds).ExecuteAsync();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Token aquired \n");
                Console.WriteLine(result.AccessToken);
                Console.ResetColor();
                if (!string.IsNullOrEmpty(result.AccessToken))
                {
                    await LoadWeatherForecastData(config, result.AccessToken);
                }
            }
            catch (MsalClientException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        private static async Task LoadWeatherForecastData(AuthConfig config, string accessToken)
        {
            var httpClient = new HttpClient();
            var defaultRequestHeaders = httpClient.DefaultRequestHeaders;
            if (defaultRequestHeaders.Accept == null ||
                defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new
                    MediaTypeWithQualityHeaderValue("application/json")
                );
            }
            defaultRequestHeaders.Authorization = new
                AuthenticationHeaderValue("bearer", accessToken);

            HttpResponseMessage response = await httpClient.GetAsync(config.BaseAddress);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("data is loaded from server");
                Console.ForegroundColor = ConsoleColor.Green;
                string json = await response.Content.ReadAsStringAsync();
                Console.WriteLine(json);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to call the Web Api: {response.StatusCode}");
                string content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Content: {content}");
            }
            Console.ResetColor();
        }
    }
}
