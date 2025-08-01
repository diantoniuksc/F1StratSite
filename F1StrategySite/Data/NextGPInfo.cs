using PexelsDotNetSDK.Api;
using System.Data;
using System.Linq;
using DotNetEnv;

namespace F1StrategySite.Data
{
    public class NextGPInfo
    {
        private Dictionary<DateTime, string> GpInfoDict { get; set; }

        private const string filePath = @"Docs\grand_prix_2025.csv";
        private DateTime Today { get; set; }
        public string GrandPrix { get; set; }

        public DateTime GpDate { get; set; }

        public NextGPInfo(DateTime today)
        {
            Today = today;
        }


        private void LoadCalendar(string filePath)
        {
            GpInfoDict = new Dictionary<DateTime, string>();

            using var reader = new StreamReader(filePath);
 
            while (true)
            {
                var row = reader.ReadLine();
                if (row == null)
                    break;

                string[] gpdata = row.Split(',');

                if (DateTime.TryParse(gpdata[1], out DateTime date)) {
                    GpInfoDict[date] = gpdata[0];
                }
            }
        }

        public string GetNextGP()
        {
            if (GpInfoDict == null)
                LoadCalendar(filePath);

            GpDate = GpInfoDict.Keys.Where(d => d > Today).OrderBy(d => d).FirstOrDefault();

            GrandPrix = GpInfoDict[GpDate];
            return GrandPrix;
        }

        public async Task<string> GetPhotoUrl()
        {
            DotNetEnv.Env.Load();
            var apiKey = Environment.GetEnvironmentVariable("PEXELS_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("PEXELS_API_KEY environment variable is not set.");
            var client = new PexelsClient(apiKey);
            


         var result = await client.SearchPhotosAsync(GrandPrix);

            var photo = result.photos.FirstOrDefault();
            Console.WriteLine(photo.source.original);

            return photo.source.original;
        }
    }
}
