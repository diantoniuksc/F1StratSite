using CsvHelper;

namespace F1StrategySite.Data
{
    public class Strategies(string name, string path = @"Docs\strategies_22-25.csv")
    {
        public string GPName { get; set; } = name;
        public string Strategy { get; set; }
        public int StrategyFrequency { get; set; }
        private string FilePath { get; set; } = path;
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
                        stratRow.StrategyFrequency = Convert.ToInt32(gpEntry[2]);
                        return stratRow;
                    }
                    return null;
                }).ToArray();

            return strategiesArr.Where(row => row != null).ToArray();
        }
    }
}
