using System;
using System.Net.Http;
using System.Configuration;
using System.IO;

namespace CloudflareLogsByDay
{
    class Program
    {
        static void Main(string[] args)
        {
            var dateToRequest = DateTime.UtcNow;
            if (args.Length == 0)
            {
                Console.WriteLine("No date specified. Assuming current day.");
            }
            if (args.Length > 0)
            {
                DateTime.TryParse(args[0], out dateToRequest);
            }
            var cloudflareUri = ConfigurationManager.AppSettings["CloudflareBaseAddress"];

            var cloudflareClient = new HttpClient
            {
                BaseAddress = new Uri(cloudflareUri)
            };
            var token = ConfigurationManager.AppSettings["CloudflareToken"];
            var email = ConfigurationManager.AppSettings["CloudflareAuthEmail"];
            cloudflareClient.DefaultRequestHeaders.Add("X-Auth-Key", token);
            cloudflareClient.DefaultRequestHeaders.Add("X-Auth-Email", email);
            var path = $"C:\\Temp\\{dateToRequest.ToString("yyyy-MM-dd")}.txt";

            var startDate = dateToRequest.Date;
            var endDate = dateToRequest.Date.AddMinutes(59).AddSeconds(59);
            var queryString = $"?start={startDate.ToString("yyyy-MM-ddTHH:mm:ssZ")}&end={endDate.ToString("yyyy-MM-ddTHH:mm:ssZ")}";

            var response = cloudflareClient.GetAsync(queryString).Result;
            if (response.IsSuccessStatusCode)
            {
                var responseString = response.Content.ReadAsStringAsync().Result;
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.Write(responseString);
                }
            }

            for (int i = 0; i < 23; i++)
            {
                startDate = startDate.AddHours(1);
                endDate = endDate.AddHours(1);
                queryString = $"?start={startDate.ToString("yyyy-MM-ddTHH:mm:ssZ")}&end={endDate.ToString("yyyy-MM-ddTHH:mm:ssZ")}";
                response = cloudflareClient.GetAsync(queryString).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseString = response.Content.ReadAsStringAsync().Result;
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.Write(responseString);
                    }
                }
            }

        }
    }
}
