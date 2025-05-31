using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace QKP.EzId.SourceGenerator.Tests.Helpers
{
    public static class SourceGeneratorTestHelper
    {
        public static GeneratorDriverRunResult RunGenerator(string source)
        {
            var (compilation, generator) = TestCompilation.CreateCompilation(source);

            // Create the driver
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

            // Run the generator
            driver = driver.RunGenerators(compilation);

            // Return the result
            return driver.GetRunResult();
        }

        public static IEnumerable<string> GetGeneratedFilePaths(this GeneratorDriverRunResult result)
        {
            return result.Results
                .SelectMany(r => r.GeneratedSources)
                .Select(s => s.HintName);
        }

        public static string GetGeneratedOutput(this GeneratorDriverRunResult result, string fileName)
        {
            var sources = result.Results
                .SelectMany(r => r.GeneratedSources)
                .Where(s => s.HintName == fileName)
                .ToList();

            if (sources.Count == 0)
            {
                return string.Empty;
            }

            return sources[0].SourceText.ToString();
        }
    }
}
