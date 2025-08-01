using F1StrategySite.Data;

namespace F1Test
{
    [TestClass]
    public sealed class TestGPInfo
    {
        [TestMethod]
        public async Task TestApi()
        {
            GPInfo info = new("Hungary", 2024);
            var apiResponse = await info.GetGpInfoAsync();
            
            Assert.IsNotNull(apiResponse);
        }

        [TestMethod]
        public async Task TestResponse()
        {
            GPInfo info = new("Hungarian Grand Prix", 2024);
            var circut = await info.GetCircuitName();

            circut.Equals("Hungaroring");
        }
    }
}
