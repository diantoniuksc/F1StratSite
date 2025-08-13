using F1StartegySite.MLModel;
using System.IO;

namespace F1StrategySite.Data
{
    public class Strategies(string name, string path = @"Docs\strategies_22-25.csv")
    {
        public string GPName { get; set; } = name;
        public string Strategy { get; set; }
        public int StrategyFrequencyInt { get; set; }
        public string StrategyFrequencyText { get; set; }
        public string FilePath { get; set; } = path;
        private static string StrategiesData { get; set; }


        private async Task GetAllStrategiesAsync()
        {
            using var reader = new StreamReader(FilePath);
            var data = await reader.ReadToEndAsync();

            StrategiesData = data;
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
                        stratRow.StrategyFrequencyText = MapFrequecyIntStr(stratRow.StrategyFrequencyInt);
                        return stratRow;
                    }
                    return null;
                }).ToArray();

            return strategiesArr.Where(row => row != null).ToArray();
        }

        private static string MapFrequecyIntStr(int numFrq)
        {
            return numFrq switch
            {
                > 0 and <= 10 => "Low Frequency",
                > 10 and <= 20 => "Medium Frequency",
                > 20 => "High Frequency",
                _ => "Unknown"
            };
        }

        public async Task<float> GetTyreLifeAsync(string tyreType, string driver_id, int start_lap)
        {
            float circuitLength = await CircutInfo.GetCircuitLengthAsync(GPName);

            MLModel1.ModelInput input = new()
            {
                Driver_id = driver_id,
                Race_length = circuitLength,
                Year = 2025,
                Compound = tyreType,
                Stint_start_lap = start_lap
            };

            var prediction = MLModel1.Predict(input);
            return prediction.Score;
        }

        public async Task<float[]> GetStrategyStintsAsync()
        {
            if(Strategy == null || Strategy.Length == 0)
                throw new InvalidOperationException("Strategy is not set or empty.");

            float[] stintLife = new float[Strategy.Length];


            for(int i = 0; i < Strategy.Length; i++)
            {
                string tyreType = Strategy[i] switch
                {
                    'S' => "SOFT",
                    'M' => "MEDIUM",
                    'H' => "HARD",
                    _ => throw new ArgumentException("Invalid tyre type")
                };

                stintLife[i] = await GetTyreLifeAsync(tyreType, "VER", 1);
            }

            return stintLife;
        }
    }
}