using CsvHelper;

namespace F1StrategySite.Data
{
    public static class CircutInfo
    {
        private const string FilePath = @"Data\CircutInfo.cs";
        public ReadFile(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<Circut>().ToList();
                // Process records as needed
            }
        }
    }
}
