using CsvHelper;
using CsvHelper.Configuration;

namespace F1StrategySite.Data
{
    public class CircuitInfoRecord
    {
        public string Name { get; set; } = string.Empty;
        public float Length { get; set; }
    }

    public class CircuitInfoRecordMap : ClassMap<CircuitInfoRecord>
    {
        public CircuitInfoRecordMap()
        {
            Map(m => m.Name).Index(0);
            Map(m => m.Length).Index(1);
        }
    }

    public static class CircutInfo
    {
    public static string? Name { get; set; }
    public static float Length { get; set; }
    private static Dictionary<string, float>? CircuitLengths { get; set; }

        private static readonly string filePath = Path.Combine(AppContext.BaseDirectory, "Docs", "circut_length.csv");

    private static async Task<Dictionary<string, float>> LoadCircuitLengths(string filePath)
        {
            if (CircuitLengths != null)
            {
                return CircuitLengths;
            }

            using var reader = new StreamReader(filePath);

            var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            };
            using var csv = new CsvReader(reader, config);

            // Map columns by index since there is no header
            csv.Context.RegisterClassMap<CircuitInfoRecordMap>();

            var circuits = await Task.Run(() => csv.GetRecords<CircuitInfoRecord>().ToList());
            CircuitLengths = circuits.ToDictionary(
                c => c.Name.Trim(),
                c => c.Length,
                StringComparer.OrdinalIgnoreCase
            );

            return CircuitLengths;
        }

        public static async Task<float> GetCircuitLengthAsync(string name)
        {
            if (CircuitLengths == null)
                await LoadCircuitLengths(filePath);
            var key = name.Trim();
            if (CircuitLengths!.TryGetValue(key, out var length))
            {
                return length;
            }
            else
            {
                throw new KeyNotFoundException($"Circuit '{name}' not found.");
            }
        }
    }
}
