using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClassMerger;

public static class Program
{
    public static void Main(string[] args)
    {
        if (TryGetProjectFolder(args, out var directoryPath) is false)
            return;

        var outputFileName = BuildOutputFileLocation(directoryPath);

        var isMinify = GetMinifyOption();

        MergeClasses(directoryPath, outputFileName, isMinify);

        CopyThumbnail(directoryPath, outputFileName);

        Console.WriteLine("Classes merged successfully!");
    }

    private static void CopyThumbnail(string directoryPath, string outputFileName)
    {
        var thumbFilePath = Path.Combine(directoryPath, "thumb.png");
        var outputDirectory = Path.GetDirectoryName(outputFileName);
        var thumbOutputPath = Path.Combine(outputDirectory!, "thumb.png");

        if (File.Exists(thumbFilePath)) File.Copy(thumbFilePath, thumbOutputPath, true);
    }

    private static bool GetMinifyOption()
    {
        Console.WriteLine("Do you want to minify the C# classes? (yes/no)");
        var minifyOption = Console.ReadLine();
        var minify = (minifyOption ?? "no").Equals("yes", StringComparison.OrdinalIgnoreCase);

        return minify;
    }

    private static string BuildOutputFileLocation(string directoryPath)
    {
        var directoryName = new DirectoryInfo(directoryPath).Name;

        var outputDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SpaceEngineers",
            "IngameScripts",
            "local",
            directoryName);

        if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);

        var outputFileName = Path.Combine(outputDirectory, "script" + ".cs");

        return outputFileName;
    }

    private static bool TryGetProjectFolder(IReadOnlyList<string> args, out string directoryPath)
    {
        if (args.FirstOrDefault() != null)
        {
            directoryPath = args[0];
        }
        else
        {
            Console.WriteLine("Enter the directory containing the Space Engineer script project:");
            directoryPath = Console.ReadLine();
        }

        if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
        {
            Console.WriteLine("Invalid directory path. Exiting...");

            return false;
        }

        return true;
    }

    private static void MergeClasses(string directoryPath, string outputFileName, bool isMinify)
    {
        var tempDirectoryPath = Path.Combine(Path.GetTempPath(), "ClassMergerTemp");
        Directory.CreateDirectory(tempDirectoryPath);

        var tempOutputFile = Path.Combine(tempDirectoryPath, outputFileName);

        WriteMergedContent(directoryPath, tempOutputFile, isMinify);

        var outputFile = Path.Combine(directoryPath, outputFileName);
        File.Move(tempOutputFile, outputFile, true);

        Directory.Delete(tempDirectoryPath, true);
    }

    private static void WriteMergedContent(string directoryPath, string tempOutputFile, bool minify)
    {
        using var outputStream = new StreamWriter(tempOutputFile);

        var files = Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            if (file.Contains(Path.Combine("obj", "")) || file.Contains(Path.Combine("bin", "")))
                continue;

            using var inputStream = new StreamReader(file);
            var fileContent = inputStream.ReadToEnd();

            fileContent = Path.GetFileName(file) == "Program.cs"
                ? StripProgramClassWrapperAndUsings(fileContent)
                : StripUsingsAndNamespaces(fileContent);

            if (minify)
                fileContent = MinifyCSharp(fileContent);

            outputStream.Write(fileContent);
            outputStream.WriteLine();
        }
    }

    private static string MinifyCSharp(string content)
    {
        // Remove single-line comments
        content = Regex.Replace(content, @"//.*$", string.Empty, RegexOptions.Multiline);

        // Remove multi-line comments
        content = Regex.Replace(content, @"/\*[\s\S]*?\*/", string.Empty, RegexOptions.Multiline);

        // Remove unnecessary white spaces
        content = Regex.Replace(content, @"\s+", " ", RegexOptions.Multiline);

        // Remove empty lines
        content = Regex.Replace(content, @"^\s*$[\r\n]*", string.Empty, RegexOptions.Multiline);

        return content;
    }

    private static string StripProgramClassWrapperAndUsings(string content)
    {
        const string CLASS_PATTERN = @"(?s)public\s+class\s+Program\s*:\s*MyGridProgram\s*{(.*)}\s*}";
        var match = Regex.Match(content, CLASS_PATTERN);

        if (match.Success && match.Groups.Count > 1)
            content = match.Groups[1].Value;

        return StripUsingsAndNamespaces(content);
    }

    private static string StripUsingsAndNamespaces(string content)
    {
        const string USING_PATTERN = @"using\s+[\w\.]+;";
        content = Regex.Replace(content, USING_PATTERN, string.Empty);

        const string NAMESPACE_PATTERN = @"namespace\s+[\w\.]+\s*{(.*)}";
        var match = Regex.Match(content, NAMESPACE_PATTERN, RegexOptions.Singleline);

        if (match.Success && match.Groups.Count > 1) content = match.Groups[1].Value;

        return content;
    }
}
