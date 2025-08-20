using F1StrategySite.Data;

namespace F1Test
{
    [TestClass]
    public sealed class TestPitstops
    {
        [TestMethod]
        public async Task TestPitstopsApi()
        {
            Pitstops pitstops = new(11, 2024);
            var (count, longest, shortest) = await pitstops.GetAllStatsAsync();

            int expectedCount = 30;
            float expectedLongest = 33.947f;
            float expectedShortest = 20.978f;

            Assert.AreEqual((expectedCount, expectedLongest, expectedShortest), (count, longest, shortest));         
        }
    }
}
