using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;

namespace Vorannoyed
{
    public sealed class VoronoiSvgExportOptions
    {
        public float Scale { get; set; } = 32f;
        public float Padding { get; set; } = 16f;
        public float SampleStep { get; set; } = 0f;
        public float SeedRadius { get; set; } = 5f;
        public float VertexRadius { get; set; } = 4f;
        public float EdgeStrokeWidth { get; set; } = 2f;
        public bool DrawSampledRegions { get; set; } = true;
        public bool DrawFiniteEdges { get; set; } = true;
        public bool DrawClippedRays { get; set; } = true;
        public bool DrawSeeds { get; set; } = true;
        public bool DrawVertices { get; set; } = true;
        public bool DrawLabels { get; set; } = false;
        public bool ExpandViewportToFitData { get; set; } = true;
    }

    public static class VoronoiSvgExporter
    {
        private const float Epsilon = 0.0001f;

        public static string ExportDebugSvg(
            VoronoiDiagram diagram,
            IReadOnlyList<Vector2> seeds,
            Vector2 boundary,
            VoronoiSvgExportOptions options = null)
        {
            if (diagram == null)
            {
                throw new ArgumentNullException(nameof(diagram));
            }

            if (seeds == null)
            {
                throw new ArgumentNullException(nameof(seeds));
            }

            options = options ?? new VoronoiSvgExportOptions();

            Vector2[] vertices = diagram.Verticies ?? Array.Empty<Vector2>();
            List<VHalfEdge> halfEdges = diagram.HalfEdges ?? new List<VHalfEdge>();

            Bounds clipBounds = Bounds.FromOriginAndExtent(boundary);
            Bounds viewBounds = options.ExpandViewportToFitData
                ? ExpandBounds(clipBounds, seeds, vertices)
                : clipBounds;

            float width = Math.Max(1f, (viewBounds.Max.X - viewBounds.Min.X) * options.Scale + options.Padding * 2f);
            float height = Math.Max(1f, (viewBounds.Max.Y - viewBounds.Min.Y) * options.Scale + options.Padding * 2f);

            StringBuilder svg = new StringBuilder();
            svg.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            svg.AppendLine(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{0}\" height=\"{1}\" viewBox=\"0 0 {0} {1}\">",
                    Format(width),
                    Format(height)));
            svg.AppendLine("  <style>");
            svg.AppendLine("    .voronoi-canvas { fill: #fffdf8; }");
            svg.AppendLine("    .voronoi-bounds { fill: none; stroke: #b8b4ac; stroke-width: 1.5; }");
            svg.AppendLine("    .voronoi-cell { opacity: 0.35; }");
            svg.AppendLine("    .voronoi-edge { fill: none; stroke: #111827; stroke-width: 2; stroke-linecap: round; }");
            svg.AppendLine("    .voronoi-ray { fill: none; stroke: #7c3aed; stroke-width: 2; stroke-dasharray: 6 4; stroke-linecap: round; }");
            svg.AppendLine("    .voronoi-seed { fill: #111827; stroke: #ffffff; stroke-width: 1.5; }");
            svg.AppendLine("    .voronoi-vertex { fill: #d61f69; stroke: #ffffff; stroke-width: 1; }");
            svg.AppendLine("    .voronoi-label { fill: #2b2b2b; font-family: Consolas, 'Courier New', monospace; font-size: 12px; }");
            svg.AppendLine("  </style>");
            svg.AppendLine(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "  <rect class=\"voronoi-canvas\" x=\"0\" y=\"0\" width=\"{0}\" height=\"{1}\" />",
                    Format(width),
                    Format(height)));

            if (options.DrawSampledRegions && seeds.Count > 0)
            {
                AppendSampledRegions(svg, seeds, clipBounds, viewBounds, options, height);
            }

            AppendBounds(svg, clipBounds, viewBounds, options, height);

            if (options.DrawFiniteEdges || options.DrawClippedRays)
            {
                AppendEdges(svg, halfEdges, vertices, clipBounds, viewBounds, options, height);
            }

            if (options.DrawSeeds)
            {
                AppendSeeds(svg, seeds, viewBounds, options, height);
            }

            if (options.DrawVertices)
            {
                AppendVertices(svg, vertices, viewBounds, options, height);
            }

            svg.AppendLine("</svg>");
            return svg.ToString();
        }

        public static void WriteDebugSvg(
            string path,
            VoronoiDiagram diagram,
            IReadOnlyList<Vector2> seeds,
            Vector2 boundary,
            VoronoiSvgExportOptions options = null)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            File.WriteAllText(path, ExportDebugSvg(diagram, seeds, boundary, options), Encoding.UTF8);
        }

        private static void AppendSampledRegions(
            StringBuilder svg,
            IReadOnlyList<Vector2> seeds,
            Bounds clipBounds,
            Bounds viewBounds,
            VoronoiSvgExportOptions options,
            float canvasHeight)
        {
            float spanX = Math.Max(clipBounds.Max.X - clipBounds.Min.X, 1f);
            float spanY = Math.Max(clipBounds.Max.Y - clipBounds.Min.Y, 1f);
            float sampleStep = options.SampleStep > 0f
                ? options.SampleStep
                : Math.Max(spanX, spanY) / 96f;

            if (sampleStep <= 0f)
            {
                sampleStep = 1f;
            }

            for (float y = clipBounds.Min.Y; y < clipBounds.Max.Y - Epsilon; y += sampleStep)
            {
                float cellHeight = Math.Min(sampleStep, clipBounds.Max.Y - y);
                for (float x = clipBounds.Min.X; x < clipBounds.Max.X - Epsilon; x += sampleStep)
                {
                    float cellWidth = Math.Min(sampleStep, clipBounds.Max.X - x);
                    Vector2 samplePoint = new Vector2(x + cellWidth * 0.5f, y + cellHeight * 0.5f);
                    int seedIndex = GetNearestSeedIndex(samplePoint, seeds);
                    string color = GetSeedColor(seedIndex);

                    svg.AppendLine(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "  <rect class=\"voronoi-cell\" x=\"{0}\" y=\"{1}\" width=\"{2}\" height=\"{3}\" fill=\"{4}\" />",
                            Format(ToSvgX(x, viewBounds, options)),
                            Format(ToSvgY(y + cellHeight, viewBounds, options, canvasHeight)),
                            Format(cellWidth * options.Scale),
                            Format(cellHeight * options.Scale),
                            color));
                }
            }
        }

        private static void AppendBounds(
            StringBuilder svg,
            Bounds clipBounds,
            Bounds viewBounds,
            VoronoiSvgExportOptions options,
            float canvasHeight)
        {
            svg.AppendLine(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "  <rect class=\"voronoi-bounds\" x=\"{0}\" y=\"{1}\" width=\"{2}\" height=\"{3}\" />",
                    Format(ToSvgX(clipBounds.Min.X, viewBounds, options)),
                    Format(ToSvgY(clipBounds.Max.Y, viewBounds, options, canvasHeight)),
                    Format((clipBounds.Max.X - clipBounds.Min.X) * options.Scale),
                    Format((clipBounds.Max.Y - clipBounds.Min.Y) * options.Scale)));
        }

        private static void AppendEdges(
            StringBuilder svg,
            List<VHalfEdge> halfEdges,
            Vector2[] vertices,
            Bounds clipBounds,
            Bounds viewBounds,
            VoronoiSvgExportOptions options,
            float canvasHeight)
        {
            HashSet<VHalfEdge> visited = new HashSet<VHalfEdge>();

            foreach (VHalfEdge halfEdge in halfEdges)
            {
                if (halfEdge == null || halfEdge.Twin == null || visited.Contains(halfEdge))
                {
                    continue;
                }

                visited.Add(halfEdge);
                visited.Add(halfEdge.Twin);

                bool halfEdgeHasVertex = IsKnownVertex(halfEdge.End, vertices);
                bool twinHasVertex = IsKnownVertex(halfEdge.Twin.End, vertices);

                if (options.DrawFiniteEdges && halfEdgeHasVertex && twinHasVertex)
                {
                    if (Vector2.DistanceSquared(halfEdge.End, halfEdge.Twin.End) > Epsilon * Epsilon)
                    {
                        AppendLine(svg, "voronoi-edge", halfEdge.End, halfEdge.Twin.End, viewBounds, options, canvasHeight);
                    }

                    continue;
                }

                if (!options.DrawClippedRays || halfEdgeHasVertex == twinHasVertex)
                {
                    continue;
                }

                VHalfEdge rayHalfEdge = halfEdgeHasVertex ? halfEdge.Twin : halfEdge;
                Vector2 rayStart = halfEdgeHasVertex ? halfEdge.End : halfEdge.Twin.End;
                Vector2 rayDirection = GetHalfEdgeDirection(rayHalfEdge);

                if (TryClipRayToBox(rayStart, rayDirection, clipBounds, out Vector2 clippedStart, out Vector2 clippedEnd))
                {
                    if (Vector2.DistanceSquared(clippedStart, clippedEnd) > Epsilon * Epsilon)
                    {
                        AppendLine(svg, "voronoi-ray", clippedStart, clippedEnd, viewBounds, options, canvasHeight);
                    }
                }
            }
        }

        private static void AppendSeeds(
            StringBuilder svg,
            IReadOnlyList<Vector2> seeds,
            Bounds viewBounds,
            VoronoiSvgExportOptions options,
            float canvasHeight)
        {
            for (int i = 0; i < seeds.Count; i++)
            {
                Vector2 seed = seeds[i];
                svg.AppendLine(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "  <circle class=\"voronoi-seed\" cx=\"{0}\" cy=\"{1}\" r=\"{2}\" fill=\"{3}\" />",
                        Format(ToSvgX(seed.X, viewBounds, options)),
                        Format(ToSvgY(seed.Y, viewBounds, options, canvasHeight)),
                        Format(options.SeedRadius),
                        GetSeedColor(i)));

                if (options.DrawLabels)
                {
                    svg.AppendLine(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "  <text class=\"voronoi-label\" x=\"{0}\" y=\"{1}\">S{2}</text>",
                            Format(ToSvgX(seed.X, viewBounds, options) + options.SeedRadius + 3f),
                            Format(ToSvgY(seed.Y, viewBounds, options, canvasHeight) - options.SeedRadius - 2f),
                            i));
                }
            }
        }

        private static void AppendVertices(
            StringBuilder svg,
            Vector2[] vertices,
            Bounds viewBounds,
            VoronoiSvgExportOptions options,
            float canvasHeight)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 vertex = vertices[i];
                svg.AppendLine(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "  <circle class=\"voronoi-vertex\" cx=\"{0}\" cy=\"{1}\" r=\"{2}\" />",
                        Format(ToSvgX(vertex.X, viewBounds, options)),
                        Format(ToSvgY(vertex.Y, viewBounds, options, canvasHeight)),
                        Format(options.VertexRadius)));

                if (options.DrawLabels)
                {
                    svg.AppendLine(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "  <text class=\"voronoi-label\" x=\"{0}\" y=\"{1}\">V{2}</text>",
                            Format(ToSvgX(vertex.X, viewBounds, options) + options.VertexRadius + 3f),
                            Format(ToSvgY(vertex.Y, viewBounds, options, canvasHeight) - options.VertexRadius - 2f),
                            i));
                }
            }
        }

        private static void AppendLine(
            StringBuilder svg,
            string cssClass,
            Vector2 start,
            Vector2 end,
            Bounds viewBounds,
            VoronoiSvgExportOptions options,
            float canvasHeight)
        {
            svg.AppendLine(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "  <line class=\"{0}\" x1=\"{1}\" y1=\"{2}\" x2=\"{3}\" y2=\"{4}\" stroke-width=\"{5}\" />",
                    cssClass,
                    Format(ToSvgX(start.X, viewBounds, options)),
                    Format(ToSvgY(start.Y, viewBounds, options, canvasHeight)),
                    Format(ToSvgX(end.X, viewBounds, options)),
                    Format(ToSvgY(end.Y, viewBounds, options, canvasHeight)),
                    Format(options.EdgeStrokeWidth)));
        }

        private static Bounds ExpandBounds(Bounds clipBounds, IReadOnlyList<Vector2> seeds, Vector2[] vertices)
        {
            float minX = clipBounds.Min.X;
            float minY = clipBounds.Min.Y;
            float maxX = clipBounds.Max.X;
            float maxY = clipBounds.Max.Y;

            for (int i = 0; i < seeds.Count; i++)
            {
                IncludePoint(seeds[i], ref minX, ref minY, ref maxX, ref maxY);
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                IncludePoint(vertices[i], ref minX, ref minY, ref maxX, ref maxY);
            }

            if (Math.Abs(maxX - minX) < Epsilon)
            {
                maxX = minX + 1f;
            }

            if (Math.Abs(maxY - minY) < Epsilon)
            {
                maxY = minY + 1f;
            }

            return new Bounds(new Vector2(minX, minY), new Vector2(maxX, maxY));
        }

        private static void IncludePoint(Vector2 point, ref float minX, ref float minY, ref float maxX, ref float maxY)
        {
            minX = Math.Min(minX, point.X);
            minY = Math.Min(minY, point.Y);
            maxX = Math.Max(maxX, point.X);
            maxY = Math.Max(maxY, point.Y);
        }

        private static int GetNearestSeedIndex(Vector2 point, IReadOnlyList<Vector2> seeds)
        {
            int bestIndex = 0;
            float bestDistance = float.PositiveInfinity;

            for (int i = 0; i < seeds.Count; i++)
            {
                float distance = Vector2.DistanceSquared(point, seeds[i]);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private static bool IsKnownVertex(Vector2 point, Vector2[] vertices)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                if (Vector2.DistanceSquared(point, vertices[i]) <= Epsilon * Epsilon)
                {
                    return true;
                }
            }

            return false;
        }

        private static Vector2 GetHalfEdgeDirection(VHalfEdge halfEdge)
        {
            if (halfEdge == null || halfEdge.Tile == null || halfEdge.Twin == null || halfEdge.Twin.Tile == null)
            {
                return Vector2.Zero;
            }

            Vector2 tileSite = halfEdge.Tile.Site;
            Vector2 twinSite = halfEdge.Twin.Tile.Site;
            Vector2 direction = new Vector2(tileSite.Y - twinSite.Y, twinSite.X - tileSite.X);

            if (direction.LengthSquared() <= Epsilon * Epsilon)
            {
                return Vector2.Zero;
            }

            direction /= (float)Math.Sqrt(direction.LengthSquared());
            return direction;
        }

        private static bool TryClipRayToBox(Vector2 origin, Vector2 direction, Bounds clipBounds, out Vector2 clippedStart, out Vector2 clippedEnd)
        {
            clippedStart = Vector2.Zero;
            clippedEnd = Vector2.Zero;

            if (direction.LengthSquared() <= Epsilon * Epsilon)
            {
                return false;
            }

            float tEnter = 0f;
            float tExit = float.PositiveInfinity;

            if (!ClipAxis(origin.X, direction.X, clipBounds.Min.X, clipBounds.Max.X, ref tEnter, ref tExit) ||
                !ClipAxis(origin.Y, direction.Y, clipBounds.Min.Y, clipBounds.Max.Y, ref tEnter, ref tExit) ||
                tExit < tEnter ||
                IsNonFinite(tEnter) ||
                IsNonFinite(tExit))
            {
                return false;
            }

            clippedStart = origin + direction * Math.Max(tEnter, 0f);
            clippedEnd = origin + direction * tExit;
            return !IsNonFinite(clippedStart.X) &&
                !IsNonFinite(clippedStart.Y) &&
                !IsNonFinite(clippedEnd.X) &&
                !IsNonFinite(clippedEnd.Y);
        }

        private static bool ClipAxis(float origin, float direction, float min, float max, ref float tEnter, ref float tExit)
        {
            if (Math.Abs(direction) < Epsilon)
            {
                return origin >= min - Epsilon && origin <= max + Epsilon;
            }

            float t1 = (min - origin) / direction;
            float t2 = (max - origin) / direction;
            float axisEnter = Math.Min(t1, t2);
            float axisExit = Math.Max(t1, t2);

            tEnter = Math.Max(tEnter, axisEnter);
            tExit = Math.Min(tExit, axisExit);
            return tExit >= tEnter;
        }

        private static float ToSvgX(float x, Bounds viewBounds, VoronoiSvgExportOptions options)
        {
            return options.Padding + (x - viewBounds.Min.X) * options.Scale;
        }

        private static float ToSvgY(float y, Bounds viewBounds, VoronoiSvgExportOptions options, float canvasHeight)
        {
            return canvasHeight - options.Padding - (y - viewBounds.Min.Y) * options.Scale;
        }

        private static bool IsNonFinite(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value);
        }

        private static string Format(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static string GetSeedColor(int index)
        {
            double hue = (index * 137.50776405003785) % 360.0;
            return HslToHex(hue / 360.0, 0.55, 0.78);
        }

        private static string HslToHex(double hue, double saturation, double lightness)
        {
            double r;
            double g;
            double b;

            if (saturation <= 0.0)
            {
                r = lightness;
                g = lightness;
                b = lightness;
            }
            else
            {
                double q = lightness < 0.5
                    ? lightness * (1.0 + saturation)
                    : lightness + saturation - lightness * saturation;
                double p = 2.0 * lightness - q;

                r = HueToRgb(p, q, hue + (1.0 / 3.0));
                g = HueToRgb(p, q, hue);
                b = HueToRgb(p, q, hue - (1.0 / 3.0));
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "#{0:X2}{1:X2}{2:X2}",
                ClampColor(r),
                ClampColor(g),
                ClampColor(b));
        }

        private static double HueToRgb(double p, double q, double t)
        {
            if (t < 0.0)
            {
                t += 1.0;
            }

            if (t > 1.0)
            {
                t -= 1.0;
            }

            if (t < 1.0 / 6.0)
            {
                return p + (q - p) * 6.0 * t;
            }

            if (t < 1.0 / 2.0)
            {
                return q;
            }

            if (t < 2.0 / 3.0)
            {
                return p + (q - p) * ((2.0 / 3.0) - t) * 6.0;
            }

            return p;
        }

        private static int ClampColor(double value)
        {
            return (int)Math.Max(0.0, Math.Min(255.0, Math.Round(value * 255.0)));
        }

        private readonly struct Bounds
        {
            public Vector2 Min { get; }
            public Vector2 Max { get; }

            public Bounds(Vector2 min, Vector2 max)
            {
                Min = min;
                Max = max;
            }

            public static Bounds FromOriginAndExtent(Vector2 extent)
            {
                return new Bounds(
                    new Vector2(Math.Min(0f, extent.X), Math.Min(0f, extent.Y)),
                    new Vector2(Math.Max(0f, extent.X), Math.Max(0f, extent.Y)));
            }
        }
    }
}
