using NUnit.Framework;
using System.Collections.Generic;
using System.Numerics;
using Vorannoyed;

namespace Vorannoyed.Tests
{
    [TestFixture]
    public class VorannoyedFactoryTests
    {
        [Test]
        public void VoronoiFactory_MakeVoronoiSFTest_SeedSpreadUniform_Passes()
        {
            //Arrange
            List<Vector2> Seeds = new List<Vector2>();
            Seeds.Add(new Vector2(2f, 2f));
            Seeds.Add(new Vector2(1f, 2f));
            Seeds.Add(new Vector2(1.5f, 1.5f));
            Seeds.Add(new Vector2(2f, 1f));
            Seeds.Add(new Vector2(1f, 1f));
            Vector2 Boundry = new Vector2(3, 3);
            Vector2[] expectedVerticies = {
                new Vector2(1.5f, 2f), new Vector2(1f, 1.5f),
                new Vector2(2f, 1.5f), new Vector2(1.5f, 1) };
            //Act
            VoronoiDiagram actual = VorannoyedFactory.MakeVoronoiSF(Seeds, Boundry);
            //Assert
            Assert.AreEqual(expectedVerticies, actual.Verticies);
        }

        [Test]
        public void VoronoiFactory_MakeVoronoiSFTest_SeedSpreadDefault_Passes()
        {
            //Arrange
            List<Vector2> Seeds = new List<Vector2>();
            Seeds.Add(new Vector2(13.9f, 6.76f));
            Seeds.Add(new Vector2(12.7f, 10.6f));
            Seeds.Add(new Vector2(8.7f, 7.7f));
            Seeds.Add(new Vector2(7.1f, 4.24f));
            Seeds.Add(new Vector2(4.6f, 11.44f));
            Vector2 Boundry = new Vector2(15f, 15f);
            Vector2[] expectedVerticies = {
                new Vector2(8.737f, 11.858f), new Vector2(11.458f, 8.104f),
                new Vector2(4.711f, 7.445f), new Vector2(10.828f, 4.616f)
            };

            //Act
            VoronoiDiagram actual = VorannoyedFactory.MakeVoronoiSF(Seeds, Boundry);
            //Assert
            Vector2[] actualVerticies = actual.Verticies;
            for (int i = 0; i < actualVerticies.Length; i++)
            {
                Assert.AreEqual(expectedVerticies[i].X, actualVerticies[i].X, 0.05);
                Assert.AreEqual(expectedVerticies[i].Y, actualVerticies[i].Y, 0.05);
            }
        }

        [Test]
        public void VoronoiFactory_MakeVoronoiSFTest_FourWayIntersect_Passes()
        {
            //Arrange
            List<Vector2> Seeds = new List<Vector2>();
            Seeds.Add(new Vector2(2.5f, 1.5f));
            Seeds.Add(new Vector2(1f, 2f));
            Seeds.Add(new Vector2(2f, 2f));
            Seeds.Add(new Vector2(1f, 1f));
            Seeds.Add(new Vector2(2f, 1f));
            Vector2 Boundry = new Vector2(15f, 15f);
            Vector2[] expectedVerticies =
            {
                new Vector2(2f, 1.5f), new Vector2(1.5f, 1.5f)
            };
            //Act
            VoronoiDiagram actual = VorannoyedFactory.MakeVoronoiSF(Seeds, Boundry);
            //Assert
            Vector2[] actualVerticies = actual.Verticies;
            for (int i = 0; i < actualVerticies.Length; i++)
            {
                Assert.AreEqual(expectedVerticies[i].X, actualVerticies[i].X, 0.05);
                Assert.AreEqual(expectedVerticies[i].Y, actualVerticies[i].Y, 0.05);
            }
        }

        [Test]
        public void VoronoiFactory_MakeVoronoiSFTest_TrapazoidResult_Passes()
        {
            //Arrange
            List<Vector2> Seeds = new List<Vector2>();
            Seeds.Add(new Vector2(9.67f, 10.75f));
            Seeds.Add(new Vector2(13.38f, 9.17f));
            Seeds.Add(new Vector2(5.62f, 8.1f));
            Seeds.Add(new Vector2(11f, 6.07f));
            Seeds.Add(new Vector2(11f, 3.06f));
            Vector2 Boundry = new Vector2(15f, 15f);
            Vector2[] expectedVerticies = {
                new Vector2(10.938f, 8.581f), new Vector2(8.627f, 7.924f),
                new Vector2(7.359f, 4.565f), new Vector2(16.169f, 4.565f)
            };
            //Act
            VoronoiDiagram actual = VorannoyedFactory.MakeVoronoiSF(Seeds, Boundry);
            //Assert
            Vector2[] actualVerticies = actual.Verticies;
            for (int i = 0; i < actualVerticies.Length; i++)
            {
                Assert.AreEqual(expectedVerticies[i].X, actualVerticies[i].X, 0.05);
                Assert.AreEqual(expectedVerticies[i].Y, actualVerticies[i].Y, 0.05);
            }
        }

        [Test]
        public void VoronoiFactory_MakeVoronoiSFTest_TwoPolygon_Passes()
        {
            //Arrange
            List<Vector2> Seeds = new List<Vector2>();
            Seeds.Add(new Vector2(10.96f, 7.53f));
            Seeds.Add(new Vector2(16.16f, 16.4f));
            Seeds.Add(new Vector2(10.43f, 10.2f));
            Seeds.Add(new Vector2(10.6f, 4.46f));
            Seeds.Add(new Vector2(4.16f, 16.67f));
            Vector2 Boundry = new Vector2(15f, 15f);
            Vector2[] expectedVerticies = {
                new Vector2(10.153f, 16.204f), new Vector2(16.786f, 10.074f),
                new Vector2(1.637f, 7.067f), new Vector2(0.695f, 7.039f), 
                new Vector2(26.985f, 4.095f)
            };
            //Act
            VoronoiDiagram actual = VorannoyedFactory.MakeVoronoiSF(Seeds, Boundry);
            //Assert
            Vector2[] actualVerticies = actual.Verticies;
            for (int i = 0; i < actualVerticies.Length; i++)
            {
                Assert.AreEqual(expectedVerticies[i].X, actualVerticies[i].X, 0.05);
                Assert.AreEqual(expectedVerticies[i].Y, actualVerticies[i].Y, 0.05);
            }
        }

        [Test]
        public void VoronoiFactory_MakeVoronoiSFTest_TwoPolygon2_Passes()
        {
            //Arrange
            List<Vector2> Seeds = new List<Vector2>();
            Seeds.Add(new Vector2(28.7f, 8.47f));
            Seeds.Add(new Vector2(38.3f, 16.12f));
            Seeds.Add(new Vector2(30.56f, 10.78f));
            Seeds.Add(new Vector2(29f, 5.2f));
            Seeds.Add(new Vector2(25.2f, 16.16f));
            Vector2 Boundry = new Vector2(15f, 15f);
            Vector2[] expectedVerticies = {
                new Vector2(31.754f, 17.329f), new Vector2(26.528f, 12.123f),
                new Vector2(32.661f, 7.185f), new Vector2(11.390f, 5.233f), 
                new Vector2(40.208f, 5.075f)
            };
            //Act
            VoronoiDiagram actual = VorannoyedFactory.MakeVoronoiSF(Seeds, Boundry);
            //Assert
            Vector2[] actualVerticies = actual.Verticies;
            for (int i = 0; i < actualVerticies.Length; i++)
            {
                Assert.AreEqual(expectedVerticies[i].X, actualVerticies[i].X, 0.05);
                Assert.AreEqual(expectedVerticies[i].Y, actualVerticies[i].Y, 0.05);
            }
        }
    }
}
