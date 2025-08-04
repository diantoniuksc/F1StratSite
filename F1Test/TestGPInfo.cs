using F1StrategySite.Data;

namespace F1Test
{
    [TestClass]
    public sealed class TestGPInfo
    {
        [TestMethod]
        public async Task TestApi()
        {
            GPInfo info = new("Bahrain Grand Prix", 2024, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\F1StrategySite\Docs\gps_laps.csv"));
            var apiResponse = await info.GetGpInfoAsync();

            Assert.IsNotNull(apiResponse);
        }

        [TestMethod]
        public async Task TestCircut()
        {
            GPInfo info = new("Hungarian Grand Prix", 2024, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\F1StrategySite\Docs\gps_laps.csv"));
            var circut = await info.GetCircuitName();

            Assert.IsTrue(circut.Equals("Hungaroring"));
        }

        [TestMethod]
        public async Task TestLocation()
        {
            GPInfo info = new("Hungarian Grand Prix", 2024, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\F1StrategySite\Docs\gps_laps.csv"));
            var location = await info.GetLocation();

            Assert.IsTrue(location.Equals("Budapest, Hungary"));
        }

        [TestMethod]
        public async Task TestDateTime()
        {
            GPInfo info = new("Hungarian Grand Prix", 2024, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\F1StrategySite\Docs\gps_laps.csv"));
            var dateTime = await info.GetRaceDateTime();

            Assert.AreEqual(new DateTime(2024, 7, 21, 13, 0, 0), dateTime);
        }

        [TestMethod]
        public async Task TestCoordinates()
        {
            GPInfo info = new("Bahrain Grand Prix", 2024, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\F1StrategySite\Docs\gps_laps.csv"));
            var map = await info.GetCircutCoordinatesAsync();

            Assert.AreEqual<(double Latitude, double Longitude)>((26.0325, 50.5106),
                (double.Parse(map.Item1), double.Parse(map.Item2)));
        }
   }
}