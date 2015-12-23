using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnlineStore.BuisnessLogic.TableManagers;

namespace OnlineStore.BuisnessLogic.UnitTests.TableManagersTests
{
    [TestClass]
    public class TableAgentTests
    {
        [TestMethod]
        public void GetPageIndex_CheckNumberPages_ReturnExpectedPagesNumbers()
        {
            var expected = new[] {1, 1, 2, 3, 4, 4};

            var agent = new TableAgent<object, object>(null);
            var actual = new[]
            {
                agent.GetPageIndex("0", 2, 4), agent.GetPageIndex("1", 2, 4), agent.GetPageIndex("2", 2, 4),
                agent.GetPageIndex("3", 2, 4), agent.GetPageIndex("4", 2, 4), agent.GetPageIndex("5", 2, 4)
            };

            Assert.AreEqual(expected.Length, actual.Length);
            for (var i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], actual[i]);
        }

        [TestMethod]
        public void GetPageIndex_CheckPrevPages_ReturnExpectedPagesNumbers()
        {
            var expected = new[] {1, 1, 2, 3};

            var agent = new TableAgent<object, object>(null);
            var actual = new[]
            {
                agent.GetPageIndex("prev", 1, 4), agent.GetPageIndex("prev", 2, 4), agent.GetPageIndex("prev", 3, 4),
                agent.GetPageIndex("prev", 4, 4)
            };

            Assert.AreEqual(expected.Length, actual.Length);
            for (var i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], actual[i]);
        }

        [TestMethod]
        public void GetPageIndex_CheckNextPages_ReturnExpectedPagesNumbers()
        {
            var expected = new[] {2, 3, 4, 4};

            var agent = new TableAgent<object, object>(null);
            var actual = new[]
            {
                agent.GetPageIndex("next", 1, 4), agent.GetPageIndex("next", 2, 4), agent.GetPageIndex("next", 3, 4),
                agent.GetPageIndex("next", 4, 4)
            };

            Assert.AreEqual(expected.Length, actual.Length);
            for (var i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], actual[i]);
        }

        [TestMethod]
        public void GetPageIndex_CheckNullPages_Return1()
        {
            const int expected = 1;

            var agent = new TableAgent<object, object>(null);
            var actuals = new[]
            {
                agent.GetPageIndex(null, 1, 4), agent.GetPageIndex(null, 2, 4), agent.GetPageIndex(null, 3, 4),
                agent.GetPageIndex(null, 4, 4)
            };

            foreach (var actual in actuals)
                Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetPageIndex_CheckNotNumberPages_ReturnOldIndexies()
        {
            var expected = new[] {1, 2, 3, 4};

            var agent = new TableAgent<object, object>(null);
            var actual = new[]
            {
                agent.GetPageIndex("a a", 1, 4), agent.GetPageIndex("a a", 2, 4), agent.GetPageIndex("a a", 3, 4),
                agent.GetPageIndex("a a", 4, 4)
            };

            Assert.AreEqual(expected.Length, actual.Length);
            for (var i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], actual[i]);
        }

        [TestMethod]
        public void GetPages_CheckNotNumberPages_ReturnExpectedPagesIndexies()
        {
            var agent = new TableAgent<object, object>(null);

            var expected = new[]
            {
                new[] {2, 3, 4},
                new[] {2, 3, 4},
                new[] {2, 3, 4},
                new[] {1, 2, 3},
                new[] {3, 4, 5, 6, 7, 8, 9, 10},
                new[] {3, 4},
                new[] {3}
            };

            var actual = new[]
            {
                agent.GetPages(2, 4, 3),
                agent.GetPages(3, 4, 3),
                agent.GetPages(4, 4, 3),
                agent.GetPages(1, 4, 3),
                agent.GetPages(3, 10, 8),
                agent.GetPages(3, 5, 2),
                agent.GetPages(3, 5, 1)
            };

            Assert.AreEqual(expected.GetLength(0), actual.Length);
            for (var i = 0; i < expected.Length; i++)
                for (var j = 0; j < expected[i].Length; j++)
                Assert.AreEqual(expected[i][j], actual[i][j]);
        }
    }
}
