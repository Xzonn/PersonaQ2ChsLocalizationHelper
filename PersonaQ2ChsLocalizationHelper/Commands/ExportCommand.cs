using Mono.Options;
using PersonaEditorLib.FileContainer;
using PersonaEditorLib.Text;
using PQ2Helper.Models;
using System.Text;
using System.Text.Json;

namespace PQ2Helper.Commands;

internal class ExportCommand : Command
{
  private string? _inputFolder, _outputFolder;

  public ExportCommand() : base("export", "Export messages files in a folder")
  {
#pragma warning disable IDE0028
    Options = new()
    {
      "Export messages files in a folder",
      "Usage: PersonaQ2ChsLocalizationHelper export -i [inputFolder] -o [outputFolder]",
      "",
      { "i|input-folder=", "Folder path to export files from", _ => _inputFolder = _ },
      { "o|output-folder=", "Folder path to export files to", _ => _outputFolder = _ },
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

    ExportARC(_inputFolder, _outputFolder);
    ExportBF(_inputFolder, _outputFolder);
    ExportBMD(_inputFolder, _outputFolder);

    return 0;
  }

  private static void ExportARC(string inputFolder, string outputFolder)
  {
    foreach (var file in Directory.GetFiles(inputFolder, "*.arc", SearchOption.AllDirectories))
    {
      Console.WriteLine($"Exporting: {file}");
      var arc = new BIN(file);
      foreach (var subFile in arc.SubFiles)
      {
        var sheetName = $"{Path.GetRelativePath(inputFolder, file)}_{subFile.Name}";
        if (subFile.GameData is BMD bmd)
        {
          ExportBMD(bmd, sheetName, outputFolder);
        }
        else if (subFile.GameData is BF bf)
        {
          ExportBF(bf, sheetName, outputFolder);
        }
      }
    }
  }

  private static void ExportBF(string inputFolder, string outputFolder)
  {
    foreach (var file in Directory.GetFiles(inputFolder, "*.bf", SearchOption.AllDirectories))
    {
      Console.WriteLine($"Exporting: {file}");
      var bf = new BF(file);
      ExportBF(bf, Path.GetRelativePath(inputFolder, file), outputFolder);
    }
  }

  private static void ExportBF(BF bf, string sheetName, string outputFolder)
  {
    foreach (var subFile in bf.SubFiles)
    {
      if (subFile.GameData is BMD bmd)
      {
        ExportBMD(bmd, sheetName, outputFolder);
      }
    }
  }

  private static void ExportBMD(string inputFolder, string outputFolder)
  {
    foreach (var file in Directory.GetFiles(inputFolder, "*.bmd", SearchOption.AllDirectories))
    {
      Console.WriteLine($"Exporting: {file}");
      var bmd = new BMD(File.ReadAllBytes(file));
      ExportBMD(bmd, Path.GetRelativePath(inputFolder, file), outputFolder);
    }
  }

  private static void ExportBMD(BMD bmd, string sheetName, string outputFolder)
  {
    var messages = new Messages
    {
      Speakers = [.. bmd.Name.Select(x => x.NameBytes.GetTextBases().GetString(Constants.ENCODING, true))],
    };

    foreach (var msg in bmd.Msg)
    {
      var message = new Message
      {
        ID = msg.Name,
        Speaker = msg.NameIndex < messages.Speakers.Count ? messages.Speakers[msg.NameIndex] : "",
        Lines = [.. msg.MsgStrings.Select(x => x.GetTextBases().GetString(Constants.ENCODING, true))],
      };
      messages.Texts.Add(message);
    }

    var newPath = Path.Combine(outputFolder, $"{sheetName}.json");
    Directory.CreateDirectory(Path.GetDirectoryName(newPath) ?? "");
    File.WriteAllText(newPath, JsonSerializer.Serialize(messages, Constants.JSON_OPTION).Replace("\\u3000", "\u3000"));
  }
}
