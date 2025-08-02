using F1StrategySite.Data;

namespace F1Test
{
    [TestClass]
    public sealed class TestGPInfo
    {
        [TestMethod]
        public async Task TestApi()
        {
            GPInfo info = new("Hungarian Grand Prix", 2024, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\F1StrategySite\Docs\gps_laps.csv"));
            var apiResponse = await info.GetGpInfoAsync();
            
            Assert.IsNotNull(apiResponse);
        }

        [TestMethod]
        public async Task TestResponse()
        {
            GPInfo info = new("Hungarian Grand Prix", 2024, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\F1StrategySite\Docs\gps_laps.csv"));
            var circut = await info.GetCircuitName();

            circut.Equals("Hungaroring");
        }
    }
}
