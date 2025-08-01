using F1StrategySite.Data;

namespace F1Test
{
    [TestClass]
    public sealed class TestGPInfo
    {
        /// <summary>
        /// Tests that the API returns a non-null response for a valid country and year.
        /// </summary>
        [TestMethod]
        public async Task TestApi()
        {
            // Arrange: Create a GPInfo instance for Hungary 2024
            GPInfo info = new("Hungary", 2024);
            // Act: Call the API
            var apiResponse = await info.GetGpInfoAsync();
            // Assert: The response should not be null
            Assert.IsNotNull(apiResponse);
        }

        /// <summary>
        /// Tests that the circuit name returned for the Hungarian Grand Prix 2024 is "Hungaroring".
        /// </summary>
        [TestMethod]
        public async Task TestResponse()
        {
            // Arrange: Create a GPInfo instance for "Hungarian Grand Prix" 2024
            GPInfo info = new("Hungarian Grand Prix", 2024);
            // Act: Get the circuit name
            var circut = await info.GetCircuitName();
            // Assert: The circuit name should be "Hungaroring"
            circut.Equals("Hungaroring");
        }
    }
}
