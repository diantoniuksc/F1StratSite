using Microsoft.AspNetCore.Mvc.Formatters;
using System.Xml.Linq;
using System.Text.Json;
using System.Runtime.CompilerServices;
using Microsoft.ML.Data;

namespace F1StrategySite.Data
{
    public class GPInfo
    {
        private Dictionary<(string Name, int Year), int> gpInfoDict { get; set; }
        private string FilePath { get; set; } = @"Docs\gps_laps.csv";
        public string GpName { get; private set; }
        public int Year { get; private set; }
        private static string GpInfo { get; set; }



        public GPInfo(string name, int year, string path = null)
        {
            GpName = name;
            Year = year;
            FilePath = path ?? @"Docs\gps_laps.csv";
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
                LoadGPInfo(FilePath);

            return gpInfoDict[(GpName + " Grand Prix", Year)];
        }



        private int GetGpNumber()
        {
            if (gpInfoDict == null)
                LoadGPInfo(FilePath);

            int round = 1;
            foreach (var key in gpInfoDict.Keys)
            {
                if (key.Year == Year)
                {
                    if (key.Name.Trim().Equals(GpName.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        return round;
                    }
                    round++;
                }
            }
            throw new Exception($"No GP found for {GpName} in {Year}");
        }

        private static HttpClient sharedClient = new()
        {
            BaseAddress = new Uri("https://api.jolpi.ca/"),
        };

        public async Task<string> GetGpInfoAsync()
        {
            // Use GetGpNumber to get the round number for this GP and year
            int round = GetGpNumber();
            // Jolpica endpoint for a specific race
            string url = $"ergast/f1/{Year}/{round}";
            using HttpResponseMessage response = await sharedClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();

            GpInfo = jsonResponse;
            return jsonResponse;
        }



        private string FindByKey(string key)
        {
            int start = GpInfo.IndexOf(key);
            if (start == -1)
                return null;

            start += key.Length;
            int end = GpInfo.IndexOf("\"", start);
            if (end == -1)
                return null;

            return GpInfo.Substring(start, end - start);
        }

        public async Task<string> GetCircuitName()
        {
            if (GpInfo == null)
                await GetGpInfoAsync();

            const string circutKey = "\"circuitName\":\"";
            string circut = FindByKey(circutKey);

            return circut;

        }

        public async Task<string> GetLocation()
        {
            if (GpInfo == null)
                await GetGpInfoAsync();

            const string keyCountry = "\"country\":\"";
            const string keyCity = "\"locality\":\"";

            string country = FindByKey(keyCountry);
            string city = FindByKey(keyCity);

            return city + ", " + country;

        }


        public async Task<DateTime> GetRaceDateTime()
        {
            if (GpInfo == null)
                await GetGpInfoAsync();

            const string dateKey = "\"date\":\"";
            const string timeKey = "\"time\":\"";

            string date = FindByKey(dateKey);
            string time = FindByKey(timeKey);

            if (date == null || time == null)
                throw new Exception("Could not find race date or time in JSON.");

            // Remove trailing 'Z' if present
            if (time.EndsWith("Z"))
                time = time.Substring(0, time.Length - 1);

            string dateTimeStr = $"{date} {time}";
            if (DateTime.TryParse(dateTimeStr, out DateTime result))
                return result;
            throw new Exception($"Could not parse date/time: {dateTimeStr}");
        }

        public async Task<(string, string)> GetCircutCoordinatesAsync()
        {
            if (GpInfo == null)
                await GetGpInfoAsync();

            string lat = FindByKey("\"lat\":\"");
            string lon = FindByKey("\"long\":\"");
            
            return (lat, lon);
        }

        public async Task<DateOnly> GetRaceWeekStartDateAsync()
        {
            if (GpInfo == null)
                await GetGpInfoAsync();

            DateOnly date = DateOnly.Parse(FindByKey("\"FirstPractice\":{\"date\":\""));

            return date;
        }
    }
}
