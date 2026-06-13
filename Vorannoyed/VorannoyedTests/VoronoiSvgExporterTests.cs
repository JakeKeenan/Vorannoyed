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

            Assert.That(svg, Does.Contain("<svg"));
            Assert.That(svg, Does.Contain("class=\"voronoi-cell\""));
            Assert.That(svg, Does.Contain("class=\"voronoi-edge\""));
            Assert.That(svg, Does.Contain("class=\"voronoi-ray\""));
            Assert.That(svg, Does.Contain("class=\"voronoi-seed\""));
            Assert.That(svg, Does.Contain("class=\"voronoi-vertex\""));
            Assert.That(svg, Does.Contain(">S0</text>"));
            Assert.That(svg, Does.Contain(">V0</text>"));
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
                Assert.That(File.ReadAllText(outputPath), Does.Contain("<svg"));
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
