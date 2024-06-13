using Mono.Options;
using PersonaEditorLib;
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

    Export(_inputFolder, _outputFolder);

    return 0;
  }

  private static void Export(string inputFolder, string outputFolder)
  {
    Helper.EnumerateFiles(inputFolder, (gameFile, sheetName) =>
    {
      if (ExportGameFile(gameFile, outputFolder, sheetName))
      {
        Console.WriteLine($"Exported: {sheetName}");
      }
    });
  }

  private static bool ExportGameFile(GameFile gameFile, string outputFolder, string sheetName)
  {
    var gameData = gameFile.GameData;
    var extension = Path.GetExtension(gameFile.Name).ToLowerInvariant();
    if (gameData is BMD bmd)
    {
      ExportBMD(bmd, outputFolder, sheetName);
      return true;
    }
    else if (Constants.EXTENSIONS_TO_EXPORT.Contains(extension))
    {
      ExportFile(gameData, outputFolder, sheetName);
      return true;
    }
    var returnValue = false;
    foreach (var subFile in gameData.SubFiles)
    {
      var subSheetName = gameData switch
      {
        BF => sheetName,
        _ => $"{sheetName}_{subFile.Name}"
      };
      returnValue = ExportGameFile(subFile, outputFolder, subSheetName) || returnValue;
    }
    return returnValue;
  }

  private static void ExportBMD(BMD bmd, string outputFolder, string sheetName)
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

  private static void ExportFile(IGameData gameData, string outputFolder, string sheetName)
  {
    var newPath = Path.Combine(outputFolder, sheetName);
    Directory.CreateDirectory(Path.GetDirectoryName(newPath) ?? "");
    File.WriteAllBytes(newPath, gameData.GetData());
  }
}
