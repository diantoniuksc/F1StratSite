using CsvHelper;
using CsvHelper.Configuration;
using System.Xml.Linq;

namespace F1StrategySite.Data
{
    public class CircuitInfoRecord
    {
        public string Name { get; set; }
        public float Length { get; set; }
        public int Year { get; set; }
    }

    public class CircuitInfoRecordMap : ClassMap<CircuitInfoRecord>
    {
        public CircuitInfoRecordMap()
        {
            Map(m => m.Name).Index(0);
            Map(m => m.Length).Index(1);
            Map(m => m.Year).Index(2);
        }
    }

    public static class CircutInfo
    {
        public static string Name { get; set; }
        public static float Length { get; set; }
        public static int Year { get; set; }
        private static Dictionary<(string Name, int Year), float> circutInfoDict { get; set; }

        private const string filePath = @"Docs\circut_length.csv";

        public static Dictionary<(string Name, int Year), float> LoadCircuitLengths(string filePath)
        {
            if (circutInfoDict != null)
            {
                return circutInfoDict;
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
            // Create dictionary: key = (Name, Year), value = Length
            circutInfoDict = circuits.ToDictionary(
                c => (c.Name.Trim(), c.Year),
                c => c.Length
            );

            return circutInfoDict;
        }

        public static float GetCircuitLength(string name, int year)
        {
            var circuitLengths = LoadCircuitLengths(filePath);
            Console.WriteLine(circuitLengths[circuitLengths.Keys.FirstOrDefault()]);
            Console.WriteLine(circuitLengths.Keys.FirstOrDefault());
            var key = (name.Trim() + " Grand Prix", year);
            if (circuitLengths.TryGetValue(key, out var length))
            {
                return length;
            }
            else
            {
                throw new KeyNotFoundException($"Circuit '{name}' for year {year} not found.");
            }
        }
    }
}
