using F1StrategySite.Data;

namespace F1Test
{
    [TestClass]
    public sealed class TestStrategies
    {
        [TestMethod]
        public async Task TestDutchStrategies()
        {
            var filePath = Path.GetFullPath(
               Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\F1StrategySite\Docs\strategies_22-25.csv")
           );

            Strategies[] targetDutchStrategies = {new Strategies("Dutch Grand Prix", filePath) { Strategy = "MH", StrategyFrequency = 13},
                                                  new Strategies("Dutch Grand Prix", filePath) { Strategy = "SMHS", StrategyFrequency = 7}};

            Strategies[] recivedDutchStrategies = await new Strategies("Dutch Grand Prix", filePath).GetStrategiesAsync();

            for (int i = 0; i < targetDutchStrategies.Length; i++)
            {
                Assert.AreEqual(targetDutchStrategies[i].GPName, recivedDutchStrategies[i].GPName);
                Assert.AreEqual(targetDutchStrategies[i].Strategy, recivedDutchStrategies[i].Strategy);
                Assert.AreEqual(targetDutchStrategies[i].StrategyFrequency, recivedDutchStrategies[i].StrategyFrequency);
            }
        }
    }
}
