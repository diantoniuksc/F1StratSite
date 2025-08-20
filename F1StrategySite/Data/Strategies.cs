using F1StrategySite.MLModel;
using System.Collections.Concurrent;
using System.IO;

namespace F1StrategySite.Data
{
    public class Strategies(string name, string path = @"Docs\strategies_22-25.csv")
    {
    public string GPName { get; set; } = name;
    public string? Strategy { get; set; }
    public int StrategyFrequencyInt { get; set; }
    public string? StrategyFrequencyText { get; set; }
    public string FilePath { get; set; } = path;
    private static string? StrategiesData { get; set; }

    // ML prediction cache
    private static readonly ConcurrentDictionary<string, float> TyreLifeCache = new();


        private async Task GetAllStrategiesAsync()
        {
            if (StrategiesData != null) return;
            using var reader = new StreamReader(FilePath);
            StrategiesData = await reader.ReadToEndAsync();
        }

        public async Task<Strategies[]> GetStrategiesAsync()
        {
            if (StrategiesData == null)
                await GetAllStrategiesAsync();

            Strategies?[] strategiesArr = StrategiesData.Split('\n')
                .Select(row =>
                {
                    string[] gpEntry = row.Split(',');
                    if (GPName == gpEntry[0])
                    {
                        Strategies stratRow = new(GPName);
                        stratRow.Strategy = gpEntry[1];
                        stratRow.StrategyFrequencyInt = Convert.ToInt32(gpEntry[2]);
                        stratRow.StrategyFrequencyText = MapFrequencyIntStr(stratRow.StrategyFrequencyInt);
                        return stratRow;
                    }
                    return null;
                }).ToArray();

            return strategiesArr.Where(row => row != null).ToArray();
        }

        private static string MapFrequencyIntStr(int numFrq)
        {
            return numFrq switch
            {
                > 0 and <= 10 => "Low Frequency",
                > 10 and <= 20 => "Medium Frequency",
                > 20 => "High Frequency",
                _ => "Unknown"
            };
        }

        public async Task<float> GetTyreLifeAsync(string tyreType, string DriverId, int startLap, int year)
        {
            string key = $"{tyreType}_{DriverId}_{startLap}_{year}_{GPName}";
            if (TyreLifeCache.TryGetValue(key, out float cachedValue))
                return cachedValue;

            float circuitLength = await CircutInfo.GetCircuitLengthAsync(GPName);

            MLModel1.ModelInput input = new()
            {
                Driver_id = DriverId,
                Race_length = circuitLength,
                Year = year,
                Compound = tyreType,
                Stint_start_lap = startLap
            };

            var prediction = MLModel1.Predict(input);
            TyreLifeCache[key] = prediction.Score;
            return prediction.Score;
        }

        public async Task<float[]> GetStrategyStintsAsync(int year)
        {
            if (string.IsNullOrEmpty(Strategy))
                throw new InvalidOperationException("Strategy is not set or empty.");

            float[] stintLife = new float[Strategy.Length];
            int stintStartLap = 1;

            for (int i = 0; i < Strategy.Length; i++)
            {
                string tyreType = Strategy[i] switch
                {
                    'S' => "SOFT",
                    'M' => "MEDIUM",
                    'H' => "HARD",
                    _ => throw new ArgumentException("Invalid tyre type")
                };

                stintLife[i] = await GetTyreLifeAsync(tyreType, "VER", stintStartLap, year);
                stintStartLap += (int)Math.Round(stintLife[i]);
            }

            return stintLife;
        }

        // Load all stint lives in parallel across strategies
        public static async Task<float[][]> LoadAllStintLivesAsync(Strategies[] strategies, int year)
        {
            var tasks = strategies.Select(s => s.GetStrategyStintsAsync(year));
            return await Task.WhenAll(tasks);
        }
    }
}