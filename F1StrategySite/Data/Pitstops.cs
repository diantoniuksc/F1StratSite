using System.Text.Json;
using System.Text.Json.Serialization;

namespace F1StrategySite.Data
{
    // Fetches pit stop data for a given Grand Prix and season
    public class Pitstops(int roundNum, int year)
    {
        public int RoundNumber { get; set; } = roundNum;
        public int Year { get; set; } = year;

        public int Count => PitStopsList?.Count ?? 0;
        public string Longest => PitStopsList?.OrderByDescending(p => float.Parse(p.Duration)).FirstOrDefault()?.ToString() ?? "No pit stops found.";
        public string Shortest => PitStopsList?.OrderBy(p => float.Parse(p.Duration)).FirstOrDefault()?.ToString() ?? "No pit stops found.";

        private static readonly HttpClient _httpClient = new HttpClient();
        private List<PitStop> PitStopsList { get; set; }

        public record PitStop(
            [property: JsonPropertyName("driverId")] string DriverId,
            [property: JsonPropertyName("lap")] string Lap,
            [property: JsonPropertyName("stop")] string Stop,
            [property: JsonPropertyName("time")] string Time,
            [property: JsonPropertyName("duration")] string Duration
        );

        // Internal DTOs for JSON binding (kept private)
        private record Root([property: JsonPropertyName("MRData")] MrData MRData);
        private record MrData([property: JsonPropertyName("RaceTable")] RaceTable RaceTable);
        private record RaceTable([property: JsonPropertyName("Races")] List<Race> Races);
        private record Race([property: JsonPropertyName("PitStops")] List<PitStop> PitStops);


        // Fetches pit stop data from the API
        public async Task GetPitstopsAsync()
        {
            string url = $"http://api.jolpi.ca/ergast/f1/{Year}/{RoundNumber}/pitstops";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var root = JsonSerializer.Deserialize<Root>(jsonResponse, options);

            PitStopsList = root?.MRData?.RaceTable?.Races?.FirstOrDefault()?.PitStops ?? [];
        }

        public async Task<int> GetTotalAsync()
        {
            if (PitStopsList == null)
            {
                await GetPitstopsAsync();
            }

            return PitStopsList.Count;
        }

        public async Task<float> GetLongestAsync()
        {
            if (PitStopsList == null)
            {
                await GetPitstopsAsync();
            }
            var longestPitStop = PitStopsList?.OrderByDescending(p => float.Parse(p.Duration)).FirstOrDefault();
            return longestPitStop != null ? float.Parse(longestPitStop.Duration) : 0;
        }

        public async Task<float> GetFastestAsync()
        {
            if (PitStopsList == null)
            {
                await GetPitstopsAsync();
            }
            var shortestPitStop = PitStopsList.OrderBy(p => float.Parse(p.Duration)).FirstOrDefault();
            return shortestPitStop != null ? float.Parse(shortestPitStop.Duration) : 0;
        }
    }
}
