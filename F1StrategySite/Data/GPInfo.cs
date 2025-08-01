using Microsoft.AspNetCore.Mvc.Formatters;
using System.Xml.Linq;
using System.Text.Json;

namespace F1StrategySite.Data
{
    public class GPInfo
    {
        private Dictionary<(string Name, int Year), int> gpInfoDict { get; set; }

        private const string filePath = @"Docs\gps_laps.csv";
        public string GpName { get; private set; }
        public int Year { get; private set; }
        private static string GpInfo { get; set; }



        public GPInfo(string name, int year)
        {
            GpName = name;
            Year = year;
        }


        private void LoadGPInfo(string filePath)
        {
            gpInfoDict = new Dictionary<(string Name, int Year), int>();

            using var reader = new StreamReader(filePath);

            while (true)
            {
                var row = reader.ReadLine();
                if (row == null)
                    break;

                string[] gpdata = row.Split(',');

                if (gpdata.Length >= 3 &&
                    double.TryParse(gpdata[2], out double totalLaps))
                {
                    gpInfoDict[(gpdata[0], int.Parse(gpdata[1]))] = (int)totalLaps;
                }
            }
        }

        public int GetTotalLaps()
        {
            if (gpInfoDict == null)
                LoadGPInfo(filePath);

            return gpInfoDict[(GpName + " Grand Prix", Year)];
        }


        private static HttpClient sharedClient = new()
        {
            BaseAddress = new Uri("https://api.openf1.org/v1/sessions"),
        };

        public async Task<string> GetGpInfoAsync()
        {
            using HttpResponseMessage response = await sharedClient.GetAsync($"?country_name={GpName}&year={Year}&session_type=Race");
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();

            GpInfo = jsonResponse;
            return jsonResponse;
        }
        
        public async Task<string> GetCircuitName()
        {
            if (GpInfo == null)
                await GetGpInfoAsync();
            
            var doc = JsonDocument.Parse(GpInfo);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
            {
                var first = root[0];
                if (first.TryGetProperty("circuit_short_name", out var circuitName))
                {
                    return circuitName.GetString();
                }
            }
            return null;
        }
    }
}
