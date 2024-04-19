using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MyApp
{
    class Program
    {
        static async Task Main(string[] args)
        {

            Console.OutputEncoding = Encoding.UTF8;



            Console.WriteLine("Jakie miasto?: ");
            var cityName = Console.ReadLine();

            string apiKeyGeo = "32b8338c9c1fdc43d190e72ea7ce9b5f";
            string geoApiUrl = $"http://api.openweathermap.org/geo/1.0/direct?q={cityName}&limit=1&appid={apiKeyGeo}";
            string apiKeyOTM = "5ae2e3f221c38a28845f05b6d1a569913da3b92ad46bf4c975cec850";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage geoResponse = await client.GetAsync(geoApiUrl);

                    if (geoResponse.IsSuccessStatusCode)
                    {
                        string geoResponseBody = await geoResponse.Content.ReadAsStringAsync();
                        var locationData = JArray.Parse(geoResponseBody);

                        if (locationData.Count > 0)
                        {
                            double cityLatitude = (double)locationData[0]["lat"];
                            double cityLongitude = (double)locationData[0]["lon"];

                            CultureInfo invariantCulture = CultureInfo.InvariantCulture;
                            string formattedLatitude = cityLatitude.ToString(invariantCulture);
                            string formattedLongitude = cityLongitude.ToString(invariantCulture);

                            string otmApiUrl = $"https://api.opentripmap.com/0.1/en/places/radius?radius=4500&lon={formattedLongitude}&lat={formattedLatitude}&apikey={apiKeyOTM}";

                            Console.WriteLine($"Querying OpenTripMap API with URL: {otmApiUrl}");
                            HttpResponseMessage otmResponse = await client.GetAsync(otmApiUrl);

                            if (otmResponse.IsSuccessStatusCode)
                            {
                                string otmResponseBody = await otmResponse.Content.ReadAsStringAsync();
                                var otmData = JObject.Parse(otmResponseBody);
                                var features = otmData["features"];

                                Console.WriteLine("Atrakcje??: ");;
                           
                                foreach (var feature in features)
                                {
                                    string type = (string)feature["properties"]["kinds"];
                                    string name = (string)feature["properties"]["name"];

                                    if (!string.IsNullOrEmpty(name) && name != "")
                                    {
                                        Console.WriteLine($"Name: {name}");
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("Failed to query OpenTripMap API.");
                                Console.WriteLine($"Status Code: {otmResponse.StatusCode}");
                                string responseBody = await otmResponse.Content.ReadAsStringAsync();
                                Console.WriteLine($"Response Body: {responseBody}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("No data found for the specified city.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to query Geocoding API.");
                        Console.WriteLine($"Status Code: {geoResponse.StatusCode}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"An error occurred: {e.Message}");
                }
            }
        }
    }
}
