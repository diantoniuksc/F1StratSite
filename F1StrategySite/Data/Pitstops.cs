using System.Text.Json;
using System.Collections.Concurrent;

namespace F1StrategySite.Data
{
    // Fetches pit stop data for a given Grand Prix and season
    public class Pitstops(int roundNum, int year)
    {
        public int RoundNumber { get; set; } = roundNum;
        public int Year { get; set; } = year;
        private List<PitStop> PitStopsList { get; set; }

        private static readonly HttpClient _httpClient = new();
        private static readonly ConcurrentDictionary<string, List<PitStop>> PitstopCache = new();

        public record PitStop(string DriverId, string Lap, string Stop, string Time, string Duration);

        private record Root(MRData MRData);
        private record MRData(RaceTable RaceTable);
        private record RaceTable(List<Race> Races);
        private record Race(List<PitStop> PitStops);

        private async Task LoadPitstopsAsync()
        {
            string key = $"{Year}_{RoundNumber}";
            if (PitstopCache.TryGetValue(key, out var cachedList))
            {
                PitStopsList = cachedList;
                return;
            }

            string url = $"http://api.jolpi.ca/ergast/f1/{Year}/{RoundNumber}/pitstops";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var root = JsonSerializer.Deserialize<Root>(jsonResponse, options);

            PitStopsList = root?.MRData?.RaceTable?.Races?.FirstOrDefault()?.PitStops ?? new List<PitStop>();
            PitstopCache[key] = PitStopsList;
        }

        // Single method to get all stats at once
        public async Task<(int Total, float Longest, float Fastest)> GetAllStatsAsync()
        {
            await LoadPitstopsAsync();
            int total = PitStopsList.Count;

            float longest = PitStopsList.Any()
                ? PitStopsList.Max(p => float.Parse(p.Duration))
                : 0;

            float fastest = PitStopsList.Any()
                ? PitStopsList.Min(p => float.Parse(p.Duration))
                : 0;

            return (total, longest, fastest);
        }
    }
}
