using Microsoft.AspNetCore.Mvc.Formatters;
using System.Xml.Linq;
using System.Text.Json;
using System.Runtime.CompilerServices;
using Microsoft.ML.Data;

namespace F1StrategySite.Data
{
    public class GPInfo(string name, int year, string path = @"Docs\gps_laps.csv", string calPath = @"Docs\grand_prix_calendar.csv")
    {
        private static Dictionary<string, int> GpInfoDict { get; set; }
        protected static Dictionary<DateTime, string> CalendarDict { get; set; }
        private string FilePath { get; set; } = path;
        protected string CalendarPath { get; set; } = calPath;
        public string GpName { get; private set; } = name;
        public int Year { get; private set; } = year;
        private static string GpInfo { get; set; }



       /* public GPInfo(string name, int year, string path = null, string calPath = null)
        {
            GpName = name;
            Year = year;
            FilePath = path ?? @"Docs\gps_laps.csv";
            CalendarPath = calPath ?? @"Docs\grand_prix_calendar.csv";
        }*/


        private static void LoadGPInfo(string filePath)
        {
            GpInfoDict = new();
            using var reader = new StreamReader(filePath);

            while (true)
            {
                var row = reader.ReadLine();
                if (row == null)
                    break;

                string[] gpdata = row.Split(',');

                if (double.TryParse(gpdata[1], out double totalLaps))
                {
                    GpInfoDict[gpdata[0]] = (int)totalLaps;
                }
            }
        }


        protected void LoadCalendar(string filePath)
        {
            CalendarDict = new();
            using var reader = new StreamReader(filePath);

            while (true)
            {
                var row = reader.ReadLine();
                if (row == null)
                    break;

                string[] gpdata = row.Split(',');

                if (DateTime.TryParse(gpdata[1], out DateTime date))
                {
                    CalendarDict[date] = gpdata[0];
                }
            }
        }



        public int GetTotalLaps()
        {
            if (GpInfoDict == null)
                LoadGPInfo(FilePath);

            return GpInfoDict[GpName];
        }

        private int GetGpNumber()
        {
            using var reader = new StreamReader(CalendarPath);
            int round = 1;
            while (true)
            {
                var row = reader.ReadLine();
                if (row == null)
                    break;
                var gpdata = row.Split(',');
                if (gpdata.Length >= 2)
                {
                    string name = gpdata[0].Trim();
                    if (name.Equals(GpName.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        return round;
                    }
                    round++;
                }
            }
            throw new Exception($"No GP found for {GpName}");
        }

        private static readonly HttpClient sharedClient = new()
        {
            BaseAddress = new Uri("https://api.jolpi.ca/"),
        };

        public async Task<string> GetGpInfoAsync()
        {
            int round = GetGpNumber();

            string url = $"ergast/f1/{Year}/{round}";
            using HttpResponseMessage response = await sharedClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();

            GpInfo = jsonResponse;
            return jsonResponse;
        }



        private static string FindByKey(string key)
        {
            int start = GpInfo.IndexOf(key);
            if (start == -1)
                return null;

            start += key.Length;
            int end = GpInfo.IndexOf("\"", start);
            if (end == -1)
                return null;

            return GpInfo[start..end];
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
            if (time.EndsWith('Z'))
                time = time[..^1];

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
