using AtlusScriptLibrary.Common.Libraries;
using AtlusScriptLibrary.Common.Logging;
using AtlusScriptLibrary.Common.Text;
using AtlusScriptLibrary.FlowScriptLanguage;
using AtlusScriptLibrary.FlowScriptLanguage.Decompiler;
using AtlusScriptLibrary.MessageScriptLanguage;
using AtlusScriptLibrary.MessageScriptLanguage.Decompiler;
using Mono.Options;
using System.Text;

namespace PQ2Helper.Commands;

internal class ExportCommand : Command
{
  private string? _inputFolder;
  public static Logger Logger = new(nameof(ExportCommand));
  public static ConsoleLogListener Listener = new(true, LogLevel.Info | LogLevel.Warning | LogLevel.Error | LogLevel.Fatal);

  public ExportCommand() : base("export", "Export messages files in a folder")
  {
#pragma warning disable IDE0028
    Options = new()
    {
      "Export messages files in a folder",
      "Usage: PersonaQ2ChsLocalizationHelper export -i [inputFolder]",
      "",
      { "i|input-folder=", "Folder path to export files from", i => _inputFolder = i },
    };
#pragma warning restore IDE0028
  }

  public override int Invoke(IEnumerable<string> arguments)
  {
    Options.Parse(arguments);
    Listener.Subscribe(Logger);
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

    ExportBF(_inputFolder);
    ExportBMD(_inputFolder);

    return 0;
  }

  private static void ExportBF(string folder)
  {
    var library = LibraryLookup.GetLibrary("pq2");
    var decompiler = new FlowScriptDecompiler
    {
      SumBits = false,
      Library = library,
    };
    decompiler.AddListener(Listener);

    foreach (var file in Directory.GetFiles(folder, "*.bf", SearchOption.AllDirectories))
    {
      Console.WriteLine($"Exporting: {file}");
      var flowScript = FlowScript.FromFile(file, Encoding.GetEncoding(932));
      decompiler.TryDecompile(flowScript, $"{file}.flow");
      decompiler.MessageScriptFilePath = null;
    }
  }

  private static void ExportBMD(string folder)
  {
    var library = LibraryLookup.GetLibrary("pq2");

    foreach (var file in Directory.GetFiles(folder, "*.bmd", SearchOption.AllDirectories))
    {
      Console.WriteLine($"Exporting: {file}");
      var script = MessageScript.FromFile(file, encoding: Encoding.GetEncoding(932));
      using var decompiler = new MessageScriptDecompiler(new FileTextWriter($"{file}.msg"))
      {
        Library = library,
      };
      decompiler.Decompile(script);
    }
  }
}
