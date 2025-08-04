using PexelsDotNetSDK.Api;
using System.Data;
using System.Linq;
using DotNetEnv;

namespace F1StrategySite.Data
{
    public class NextGPInfo : GPInfo
    {
        private DateTime Today { get; set; }
        public string GrandPrix { get; set; }
        public DateTime GpDate { get; set; }

        public NextGPInfo(DateTime today, string calPath = null) : base("", 0)
        {
            Today = today;
            if (CalendarDict == null)
                LoadCalendar(CalendarPath);
        }

        public string GetNextGP()
        {
            GpDate = CalendarDict.Keys.Where(d => d > Today).OrderBy(d => d).FirstOrDefault();
            GrandPrix = CalendarDict[GpDate];
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
