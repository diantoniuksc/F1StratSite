using PexelsDotNetSDK.Api;
using System.Data;
using System.Linq;
using DotNetEnv;

namespace F1StrategySite.Data
{
    public class NextGPInfo : GPInfo
    {
        private DateTime Today { get; set; }
        public string? GrandPrix { get; set; }
        public DateTime GpDate { get; set; }

        public NextGPInfo(DateTime today, string? calPath = null) : base("", 0, null, calPath)
        {
            Today = today;
            if (CalendarDict == null)
                LoadCalendar(CalendarPath);
        }

        public string GetNextGP()
        {
            if (CalendarDict == null || CalendarDict.Count == 0)
            {
                return string.Empty;
            }
            GpDate = CalendarDict.Keys.Where(d => d > Today).OrderBy(d => d).FirstOrDefault();
            GrandPrix = CalendarDict![GpDate];
            return GrandPrix ?? string.Empty;
        }

        public async Task<string> GetPhotoUrl()
        {
            DotNetEnv.Env.Load();
            var apiKey = Environment.GetEnvironmentVariable("PEXELS_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                // Fallback stock image if API key not present in Azure
                return "https://images.pexels.com/photos/31204633/pexels-photo-31204633.jpeg";
            }
            var client = new PexelsClient(apiKey);

            var query = string.IsNullOrWhiteSpace(GrandPrix) ? "Formula 1" : GrandPrix;
            var result = await client.SearchPhotosAsync(query);
            var photo = result.photos.FirstOrDefault();
            return photo?.source?.original ?? "https://images.pexels.com/photos/31204633/pexels-photo-31204633.jpeg";
        }
    }
}