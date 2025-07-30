namespace F1StrategySite.Data
{
    public static class GPInfo
    {
        private static Dictionary<(string Name, int Year), int> gpInfoDict { get; set; }

        private const string filePath = @"Docs\gps_laps.csv";

        private static void LoadGPInfo(string filePath)
        {
            gpInfoDict = new Dictionary<(string Name, int Year), int>();

            using var reader = new StreamReader(filePath);

            while (true)
            {
                var row = reader.ReadLine();
                if (row == null)
                    break;

                string[] gpdata = row.Split(',');

                if (gpdata.Length >= 3 &&
                    double.TryParse(gpdata[2], out double totalLaps))
                {
                    gpInfoDict[(gpdata[0], int.Parse(gpdata[1]))] = (int)totalLaps;
                }
            }
        }

        public static int GetTotalLaps(string name, int year)
        {
            if (gpInfoDict == null)
                LoadGPInfo(filePath);

            return gpInfoDict[(name + " Grand Prix", year)];
        }
    }
}
