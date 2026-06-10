using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Vorannoyed;

namespace Vorannoyed.Tests
{
    [TestFixture]
    public class VoronoiSvgExporterTests
    {
        [Test]
        public void VoronoiSvgExporter_ExportDebugSvg_ContainsExpectedLayers()
        {
            List<Vector2> seeds = new List<Vector2>
            {
                new Vector2(2f, 2f),
                new Vector2(1f, 2f),
                new Vector2(1.5f, 1.5f),
                new Vector2(2f, 1f),
                new Vector2(1f, 1f),
            };

            Vector2 boundary = new Vector2(3f, 3f);
            VoronoiDiagram diagram = VorannoyedFactory.MakeVoronoiSF(seeds, boundary);

            string svg = VoronoiSvgExporter.ExportDebugSvg(
                diagram,
                seeds,
                boundary,
                new VoronoiSvgExportOptions { DrawLabels = true });

            StringAssert.Contains("<svg", svg);
            StringAssert.Contains("class=\"voronoi-cell\"", svg);
            StringAssert.Contains("class=\"voronoi-edge\"", svg);
            StringAssert.Contains("class=\"voronoi-ray\"", svg);
            StringAssert.Contains("class=\"voronoi-seed\"", svg);
            StringAssert.Contains("class=\"voronoi-vertex\"", svg);
            StringAssert.Contains(">S0</text>", svg);
            StringAssert.Contains(">V0</text>", svg);
        }

        [Test]
        public void VoronoiSvgExporter_WriteDebugSvg_WritesSvgFile()
        {
            List<Vector2> seeds = new List<Vector2>
            {
                new Vector2(13.9f, 6.76f),
                new Vector2(12.7f, 10.6f),
                new Vector2(8.7f, 7.7f),
                new Vector2(7.1f, 4.24f),
                new Vector2(4.6f, 11.44f),
            };

            Vector2 boundary = new Vector2(15f, 15f);
            VoronoiDiagram diagram = VorannoyedFactory.MakeVoronoiSF(seeds, boundary);
            string outputPath = Path.Combine(Path.GetTempPath(), $"vorannoyed-{Guid.NewGuid():N}.svg");

            try
            {
                VoronoiSvgExporter.WriteDebugSvg(outputPath, diagram, seeds, boundary);

                Assert.That(File.Exists(outputPath), Is.True);
                StringAssert.Contains("<svg", File.ReadAllText(outputPath));
            }
            finally
            {
                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }
            }
        }
    }
}
