using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using Vorannoyed;

namespace VorannoyedSvgRunner
{
    internal static class Program
    {
        private static readonly Dictionary<string, Scenario> Scenarios =
            new Dictionary<string, Scenario>(StringComparer.OrdinalIgnoreCase)
            {
                ["uniform"] = new Scenario(
                    "uniform",
                    new Vector2(3f, 3f),
                    new[]
                    {
                        new Vector2(2f, 2f),
                        new Vector2(1f, 2f),
                        new Vector2(1.5f, 1.5f),
                        new Vector2(2f, 1f),
                        new Vector2(1f, 1f),
                    }),
                ["default"] = new Scenario(
                    "default",
                    new Vector2(15f, 15f),
                    new[]
                    {
                        new Vector2(13.9f, 6.76f),
                        new Vector2(12.7f, 10.6f),
                        new Vector2(8.7f, 7.7f),
                        new Vector2(7.1f, 4.24f),
                        new Vector2(4.6f, 11.44f),
                    }),
                ["four-way"] = new Scenario(
                    "four-way",
                    new Vector2(15f, 15f),
                    new[]
                    {
                        new Vector2(2.5f, 1.5f),
                        new Vector2(1f, 2f),
                        new Vector2(2f, 2f),
                        new Vector2(1f, 1f),
                        new Vector2(2f, 1f),
                    }),
                ["trapezoid"] = new Scenario(
                    "trapezoid",
                    new Vector2(15f, 15f),
                    new[]
                    {
                        new Vector2(9.67f, 10.75f),
                        new Vector2(13.38f, 9.17f),
                        new Vector2(5.62f, 8.1f),
                        new Vector2(11f, 6.07f),
                        new Vector2(11f, 3.06f),
                    }),
                ["two-polygon"] = new Scenario(
                    "two-polygon",
                    new Vector2(28f, 18f),
                    new[]
                    {
                        new Vector2(10.96f, 7.53f),
                        new Vector2(16.16f, 16.4f),
                        new Vector2(10.43f, 10.2f),
                        new Vector2(10.6f, 4.46f),
                        new Vector2(4.16f, 16.67f),
                    }),
                ["two-polygon2"] = new Scenario(
                    "two-polygon2",
                    new Vector2(42f, 18f),
                    new[]
                    {
                        new Vector2(28.7f, 8.47f),
                        new Vector2(38.3f, 16.12f),
                        new Vector2(30.56f, 10.78f),
                        new Vector2(29f, 5.2f),
                        new Vector2(25.2f, 16.16f),
                    }),
            };

        private static int Main(string[] args)
        {
            try
            {
                if (TryHandleMetaCommand(args))
                {
                    return 0;
                }

                string scenarioName = "uniform";
                string? outputPath = null;
                float scale = 48f;
                float sampleStep = 0f;
                bool drawLabels = true;
                bool drawHalfEdgeDirections = false;

                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i];
                    switch (arg)
                    {
                        case "--output":
                        case "-o":
                            outputPath = GetRequiredValue(args, ref i, arg);
                            break;
                        case "--scale":
                            scale = ParseFloat(GetRequiredValue(args, ref i, arg), arg);
                            break;
                        case "--sample-step":
                            sampleStep = ParseFloat(GetRequiredValue(args, ref i, arg), arg);
                            break;
                        case "--labels":
                            drawLabels = true;
                            break;
                        case "--no-labels":
                            drawLabels = false;
                            break;
                        case "--half-edges":
                        case "--half-edge-directions":
                            drawHalfEdgeDirections = true;
                            break;
                        default:
                            if (arg.StartsWith("-", StringComparison.Ordinal))
                            {
                                Console.Error.WriteLine($"Unknown option: {arg}");
                                PrintUsage();
                                return 1;
                            }

                            scenarioName = arg;
                            break;
                    }
                }

                if (!Scenarios.TryGetValue(scenarioName, out Scenario? scenario) || scenario == null)
                {
                    Console.Error.WriteLine($"Unknown scenario '{scenarioName}'.");
                    PrintUsage();
                    return 1;
                }

                outputPath = string.IsNullOrWhiteSpace(outputPath)
                    ? Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, $"{scenario.Name}.svg"))
                    : Path.GetFullPath(outputPath);

                string? directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                VoronoiDiagram diagram = VorannoyedFactory.MakeVoronoiSF(
                    new List<Vector2>(scenario.Seeds),
                    scenario.Boundary);

                VoronoiSvgExportOptions options = new VoronoiSvgExportOptions
                {
                    DrawLabels = drawLabels,
                    DrawHalfEdgeDirections = drawHalfEdgeDirections,
                    Scale = scale,
                    SampleStep = sampleStep,
                };

                VoronoiSvgExporter.WriteDebugSvg(outputPath, diagram, scenario.Seeds, scenario.Boundary, options);

                Console.WriteLine($"Wrote SVG for '{scenario.Name}' to:");
                Console.WriteLine(outputPath);
                return 0;
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine(ex.Message);
                PrintUsage();
                return 1;
            }
        }

        private static bool TryHandleMetaCommand(string[] args)
        {
            if (args.Length == 0)
            {
                return false;
            }

            string firstArg = args[0];
            if (string.Equals(firstArg, "--help", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(firstArg, "-h", StringComparison.OrdinalIgnoreCase))
            {
                PrintUsage();
                return true;
            }

            if (string.Equals(firstArg, "--list", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Available scenarios:");
                foreach (string scenarioName in Scenarios.Keys)
                {
                    Console.WriteLine($"  {scenarioName}");
                }

                return true;
            }

            return false;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  dotnet run --project VorannoyedSvgRunner [scenario] [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --output, -o <path>      Output SVG path");
            Console.WriteLine("  --scale <number>         Pixels per world unit");
            Console.WriteLine("  --sample-step <number>   Background sample size in world units");
            Console.WriteLine("  --labels                 Draw labels for seeds and vertices");
            Console.WriteLine("  --no-labels              Hide labels");
            Console.WriteLine("  --half-edges             Draw colored directed half-edge overlays");
            Console.WriteLine("  --list                   Show built-in scenarios");
            Console.WriteLine("  --help, -h               Show this help");
            Console.WriteLine();
            Console.WriteLine("Default scenario: uniform");
        }

        private static string GetRequiredValue(string[] args, ref int index, string optionName)
        {
            if (index + 1 >= args.Length)
            {
                throw new ArgumentException($"Missing value for {optionName}.");
            }

            index++;
            return args[index];
        }

        private static float ParseFloat(string value, string optionName)
        {
            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedValue))
            {
                throw new ArgumentException($"Invalid numeric value '{value}' for {optionName}.");
            }

            return parsedValue;
        }

        private sealed class Scenario
        {
            public string Name { get; }
            public Vector2 Boundary { get; }
            public IReadOnlyList<Vector2> Seeds { get; }

            public Scenario(string name, Vector2 boundary, IReadOnlyList<Vector2> seeds)
            {
                Name = name;
                Boundary = boundary;
                Seeds = seeds;
            }
        }
    }
}
