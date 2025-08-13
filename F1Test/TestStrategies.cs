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

            Strategies[] targetDutchStrategies = {new Strategies("Dutch Grand Prix", filePath) { Strategy = "MH", StrategyFrequencyInt = 13},
                                                  new Strategies("Dutch Grand Prix", filePath) { Strategy = "SMHS", StrategyFrequencyInt = 7}};

            Strategies[] actualDutchStrategies = await new Strategies("Dutch Grand Prix", filePath).GetStrategiesAsync();

            for (int i = 0; i < targetDutchStrategies.Length; i++)
            {
                Assert.AreEqual(targetDutchStrategies[i].GPName, actualDutchStrategies[i].GPName);
                Assert.AreEqual(targetDutchStrategies[i].Strategy, actualDutchStrategies[i].Strategy);
                Assert.AreEqual(targetDutchStrategies[i].StrategyFrequencyInt, actualDutchStrategies[i].StrategyFrequencyInt);
            }
        }

        [TestMethod]
        public async Task TestDutchStints()
        {
            var filePath = Path.GetFullPath(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Docs\strategies_22-25.csv")
           );

            Strategies[] actualDutchStrategies = await new Strategies("Dutch Grand Prix", filePath).GetStrategiesAsync();
            Strategies actualStrategy = new Strategies(actualDutchStrategies[0].GPName, filePath)
            {
                Strategy = actualDutchStrategies[0].Strategy,
                StrategyFrequencyInt = actualDutchStrategies[0].StrategyFrequencyInt
            };

            float[] actualTyreLifes = await actualStrategy.GetStrategyStintsAsync();
            float[] expectedTyreLifes = { 25.46488f, 37.05885f };

            CollectionAssert.AreEqual(expectedTyreLifes, actualTyreLifes);
        }
    }
}
