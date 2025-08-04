using CsvHelper;
using CsvHelper.Configuration;

namespace F1StrategySite.Data
{
    public class CircuitInfoRecord
    {
        public string Name { get; set; }
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
        public static string Name { get; set; }
        public static float Length { get; set; }
        private static Dictionary<string, float> CircuitLengths { get; set; }

        private const string filePath = @"Docs\circut_length.csv";

        private static Dictionary<string, float> LoadCircuitLengths(string filePath)
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

            var circuits = csv.GetRecords<CircuitInfoRecord>().ToList();
            CircuitLengths = circuits.ToDictionary(
                c => c.Name.Trim(),
                c => c.Length
            );

            return CircuitLengths;
        }

        public static float GetCircuitLength(string name)
        {
            var circuitLengths = LoadCircuitLengths(filePath);
            var key = name.Trim() + " Grand Prix";
            if (circuitLengths.TryGetValue(key, out var length))
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
