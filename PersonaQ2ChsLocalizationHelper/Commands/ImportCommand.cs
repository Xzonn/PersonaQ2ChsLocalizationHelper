using AtlusScriptLibrary.Common.Libraries;
using AtlusScriptLibrary.Common.Logging;
using AtlusScriptLibrary.Common.Text.Encodings;
using AtlusScriptLibrary.FlowScriptLanguage.Compiler;
using AtlusScriptLibrary.MessageScriptLanguage.Compiler;
using Mono.Options;
using System.Text;

namespace PQ2Helper.Commands;

internal class ImportCommand : Command
{
  private string? _inputFolder, _outputFolder;
  public static Logger Logger = new(nameof(ImportCommand));
  public static ConsoleLogListener Listener = new(true, LogLevel.Warning | LogLevel.Error | LogLevel.Fatal);

  public ImportCommand() : base("import", "Import messages files in a folder")
  {
#pragma warning disable IDE0028
    Options = new()
    {
      "Import messages files in a folder",
      "Usage: PersonaQ2ChsLocalizationHelper import -i [inputFolder] -o [outputFolder]",
      "",
      { "i|input-folder=", "Folder path to import files from", i => _inputFolder = i },
      { "o|output-folder=", "Folder path to save imported files", o => _outputFolder = o },
    };
#pragma warning restore IDE0028
  }

  public override int Invoke(IEnumerable<string> arguments)
  {
    Options.Parse(arguments);
    if (string.IsNullOrEmpty(_inputFolder))
    {
      if (Environment.GetEnvironmentVariable("PQ2_ROOT") is string pq2_root && !string.IsNullOrEmpty(pq2_root))
      {
        _inputFolder = pq2_root;
      }
      else
      {
        _inputFolder = Directory.GetCurrentDirectory();
      }
    }
    if (string.IsNullOrEmpty(_outputFolder))
    {
      _outputFolder = _inputFolder;
    }

    ImportBF(_inputFolder, _outputFolder);
    ImportBMD(_inputFolder, _outputFolder);

    return 0;
  }

  private static void ImportBF(string folder, string outputFolder)
  {
    var library = LibraryLookup.GetLibrary("pq2");
    var version = AtlusScriptLibrary.FlowScriptLanguage.FormatVersion.Version2;
    var compiler = new FlowScriptCompiler(version)
    {
      Encoding = Encoding.GetEncoding(932),
      ProcedureHookMode = ProcedureHookMode.None,
      Library = library,
    };
    compiler.AddListener(Listener);

    foreach (var file in Directory.GetFiles(folder, "*.bf", SearchOption.AllDirectories))
    {
      if (!(File.Exists($"{file}.flow") && File.Exists($"{file}.msg"))) { continue; }
      Console.WriteLine($"Importing: {file}");
      try
      {
        if (!compiler.TryCompile(File.OpenRead($"{file}.flow"), out var flowScript))
        {
          Console.Error.WriteLine("One or more errors occured during compilation!");
          continue;
        }

        var newPath = Path.Combine(outputFolder, Path.GetRelativePath(folder, file));
        flowScript.ToFile(newPath);
      }
      catch (UnsupportedCharacterException e)
      {
        Console.Error.WriteLine($"Character '{e.Character}' not supported by encoding '{e.EncodingName}'");
      }
    }
  }

  private static void ImportBMD(string folder, string outputFolder)
  {
    var library = LibraryLookup.GetLibrary("pq2");
    var version = AtlusScriptLibrary.MessageScriptLanguage.FormatVersion.Version1;
    var compiler = new MessageScriptCompiler(version, Encoding.GetEncoding(932))
    {
      Library = library
    };

    foreach (var file in Directory.GetFiles(folder, "*.bmd", SearchOption.AllDirectories))
    {
      if (!File.Exists($"{file}.msg")) { continue; }
      Console.WriteLine($"Importing: {file}");

      try
      {
        if (!compiler.TryCompile(File.OpenText($"{file}.msg"), out var script))
        {
          Console.Error.WriteLine("One or more errors occured during compilation!");
        }

        var newPath = Path.Combine(outputFolder, Path.GetRelativePath(folder, file));
        script.ToFile(newPath);
      }
      catch (UnsupportedCharacterException e)
      {
        Console.Error.WriteLine($"Character '{e.Character}' not supported by encoding '{e.EncodingName}'");
      }
    }
  }
}
