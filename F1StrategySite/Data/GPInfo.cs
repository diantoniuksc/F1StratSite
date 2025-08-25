using Microsoft.AspNetCore.Mvc.Formatters;
using System.Xml.Linq;
using System.Text.Json;
using System.Runtime.CompilerServices;
using Microsoft.ML.Data;

namespace F1StrategySite.Data
{
    public class GPInfo(string name, int year, string? path = null, string? calPath = null)
    {
        private static Dictionary<string, int>? GpInfoDict { get; set; }
        protected static Dictionary<DateTime, string>? CalendarDict { get; set; }

        private static readonly Dictionary<int, (string[] gpNames, string[] circuits)> ScheduleCache
            = [];
    private string FilePath { get; set; } = path ?? Path.Combine(AppContext.BaseDirectory, "Docs", "gps_laps.csv");
    protected string CalendarPath { get; set; } = calPath ?? Path.Combine(AppContext.BaseDirectory, "Docs", "grand_prix_calendar.csv");
        public string GpName { get; private set; } = name;
        public int Year { get; private set; } = year;
    private static string? GpInfo { get; set; }



       /* public GPInfo(string name, int year, string path = null, string calPath = null)
        {
            GpName = name;
            Year = year;
            FilePath = path ?? @"Docs\gps_laps.csv";
            CalendarPath = calPath ?? @"Docs\grand_prix_calendar.csv";
        }*/


        private static void LoadGPInfo(string filePath)
        {
            GpInfoDict = [];
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
            CalendarDict = [];
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

            if (GpInfoDict!.TryGetValue(GpName, out var laps))
            {
                return laps;
            }
            throw new KeyNotFoundException($"Grand Prix '{GpName}' not found in '{FilePath}'.");
        }

       /* public int GetGpNumber()
        {
            using var reader = new StreamReader(CalendarPath);
            int round = 1;
            int year = 2022;
            while (true)
            {
                var row = reader.ReadLine();
                if (row == null)
                    break;
                var gpdata = row.Split(',');
                if (gpdata.Length >= 2)
                {
                    int currentYear = Convert.ToInt32(gpdata[1].Split('-')[0]);

                    if(currentYear != year)
                    {
                        round = 1;
                        year = currentYear;
                    }
                    string name = gpdata[0].Trim();
                    if (name.Equals(GpName.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        Year = year;
                        return round;
                    }
                    round++;
                }
            }
            throw new Exception($"No GP found for {GpName}");
        }*/

        private static readonly HttpClient sharedClient = new()
        {
            BaseAddress = new Uri("https://api.jolpi.ca/"),
        };

        public async Task<string> GetGpInfoAsync()
        {
            int round = await GetGpRoundAsync();

            string url = $"ergast/f1/{Year}/{round}";
            using HttpResponseMessage response = await sharedClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();

            GpInfo = jsonResponse;
            return jsonResponse;
        }



        private static string? FindByKey(string key)
        {
            if (string.IsNullOrEmpty(GpInfo)) return null;
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
            string? circut = FindByKey(circutKey);

            return circut ?? string.Empty;

        }

        public async Task<string> GetLocation()
        {
            if (GpInfo == null)
                await GetGpInfoAsync();

            const string keyCountry = "\"country\":\"";
            const string keyCity = "\"locality\":\"";

            string? country = FindByKey(keyCountry);
            string? city = FindByKey(keyCity);

            return $"{city ?? ""}{(string.IsNullOrEmpty(city) || string.IsNullOrEmpty(country) ? "" : ", ")}{country ?? ""}";

        }


        public async Task<DateTime> GetRaceDateTime()
        {
            if (GpInfo == null)
                await GetGpInfoAsync();

            const string dateKey = "\"date\":\"";
            const string timeKey = "\"time\":\"";

            string? date = FindByKey(dateKey);
            string? time = FindByKey(timeKey);

            if (date == null || time == null)
                throw new Exception("Could not find race date or time in JSON.");

            // Compose ISO-8601 and parse as UTC (ergast returns UTC times with trailing 'Z')
            string iso = $"{date}T{time}"; // e.g., 2025-09-14T13:00:00Z
            if (DateTimeOffset.TryParse(iso, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                out var dto))
            {
                return dto.UtcDateTime; // always UTC
            }
            throw new Exception($"Could not parse date/time: {iso}");
        }

        public async Task<(string, string)> GetCircutCoordinatesAsync()
        {
            if (GpInfo == null)
                await GetGpInfoAsync();

            string? lat = FindByKey("\"lat\":\"");
            string? lon = FindByKey("\"long\":\"");
            
            return (lat ?? string.Empty, lon ?? string.Empty);
        }

        public async Task<DateOnly> GetRaceWeekStartDateAsync()
        {
            if (GpInfo == null)
                await GetGpInfoAsync();

            var fp = FindByKey("\"FirstPractice\":{\"date\":\"");
            if (string.IsNullOrEmpty(fp))
                throw new Exception("Could not find FirstPractice date.");
            DateOnly date = DateOnly.Parse(fp);

            return date;
        }



        public static readonly List<string> DriverIds = new List<string>
        {
            "HAM",
            "VER",
            "LEC",
            "NOR",
            "SAI",
            "RUS",
            "ALO",
            "PER",
            "PIA",
            "GAS",
            "OCO",
            "TSU",
            "BOT",
            "STR",
            "ALB",
            "HUL",
            "SAR",
            "ZHO",
            "MAG",
            "RIC",
            "COL",
            "DOO",
            "ANT",
            "BOR",
            "HAD",
            "LAW",
            "VET",
            "RAI",
            "LAT",
            "GIO",
            "SCH",
            "KUB",
            "DEV",
            "BEA"
        };

        public static async Task<(string[], string[])> GetSchedule(int year)
        {
            string url = $"ergast/f1/{year}/races";
            using HttpResponseMessage response = await sharedClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(jsonResponse);
            var races = doc.RootElement
                .GetProperty("MRData")
                .GetProperty("RaceTable")
                .GetProperty("Races");

            var gpNames = new List<string>();
            var circuits = new List<string>();
            foreach (var race in races.EnumerateArray())
            {
                if (race.TryGetProperty("raceName", out var raceName))
                {
                    var val = raceName.GetString();
                    if (!string.IsNullOrEmpty(val)) gpNames.Add(val);
                }

                if (race.TryGetProperty("Circuit", out var circuit) && circuit.TryGetProperty("circuitName", out var circuitName))
                {
                    var val = circuitName.GetString();
                    if (!string.IsNullOrEmpty(val)) circuits.Add(val);
                }
            }
            return (gpNames.ToArray(), circuits.ToArray());
        }

        public async Task<int> GetGpRoundAsync()
        {
            if (!ScheduleCache.TryGetValue(Year, out var schedule))
            {
                schedule = await GetSchedule(Year);
                ScheduleCache[Year] = schedule;
            }

            var gpNames = schedule.gpNames;

            // Find the GP round (index + 1)
            for (int i = 0; i < gpNames.Length; i++)
            {
                if (gpNames[i].Trim().Equals(GpName.Trim(), StringComparison.OrdinalIgnoreCase))
                    return i + 1;
            }

            throw new Exception($"No GP found for {GpName} in {Year}");
        }
    }
}
