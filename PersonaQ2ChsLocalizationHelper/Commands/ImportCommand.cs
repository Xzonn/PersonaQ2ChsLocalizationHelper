using Mono.Options;
using PersonaEditorLib;
using PersonaEditorLib.FileContainer;
using PersonaEditorLib.Text;
using PQ2Helper.Models;
using System.Text.Json;

namespace PQ2Helper.Commands;

internal class ImportCommand : Command
{
  private string? _inputFolder, _jsonFolder, _outputFolder;

  public ImportCommand() : base("import", "Import messages files in a folder")
  {
#pragma warning disable IDE0028
    Options = new()
    {
      "Import messages files in a folder",
      "Usage: PersonaQ2ChsLocalizationHelper import -i [inputFolder] -j [jsonFolder] -o [outputFolder]",
      "",
      { "i|input-folder=", "Folder path to import files from", _ => _inputFolder = _ },
      { "j|json-folder=", "Folder path to json files", _ => _jsonFolder = _ },
      { "o|output-folder=", "Folder path to save imported files", _ => _outputFolder = _ },
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
    if (string.IsNullOrEmpty(_jsonFolder))
    {
      _jsonFolder = _inputFolder;
    }
    if (string.IsNullOrEmpty(_outputFolder))
    {
      _outputFolder = _inputFolder;
    }

    Import(_inputFolder, _jsonFolder, _outputFolder);

    return 0;
  }

  private static void Import(string inputFolder, string jsonFolder, string outputFolder)
  {
    Helper.EnumerateFiles(inputFolder, (gamefile, sheetName) =>
    {
      if (ImportFile(gamefile.GameData, outputFolder, sheetName, jsonFolder))
      {
        var outputPath = Path.Combine(outputFolder, sheetName);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? "");
        File.WriteAllBytes(outputPath, gamefile.GameData.GetData());

        Console.WriteLine($"Imported: {sheetName}");
      }
    });
  }

  private static bool ImportFile(IGameData gameData, string outputFolder, string sheetName, string jsonFolder)
  {
    if (gameData is BMD bmd)
    {
      ImportBMD(ref bmd, sheetName, jsonFolder);
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
      returnValue = ImportFile(subFile.GameData, outputFolder, subSheetName, jsonFolder) || returnValue;
    }
    return returnValue;
  }

  private static void ImportBMD(ref BMD bmd, string sheetName, string jsonFolder)
  {
    var jsonPath = Path.Combine(jsonFolder, $"{sheetName}.json");
    if (!File.Exists(jsonPath)) { return; }
    var messages = JsonSerializer.Deserialize<Messages>(File.ReadAllText(jsonPath));
    if (messages is null) { return; }

    for (var i = 0; i < bmd.Msg.Count; i++)
    {
      var lines = messages.Texts[i].Lines;
      for (var j = 0; j < bmd.Msg[i].MsgStrings.Length; j++)
      {
        bmd.Msg[i].MsgStrings[j] = lines[j].GetTextBases(Constants.ENCODING).GetByteArray();
      }
    }

    for (var i = 0; i < bmd.Name.Count; i++)
    {
      var speaker = messages.Speakers[i];
      bmd.Name[i].NameBytes = speaker.GetTextBases(Constants.ENCODING).GetByteArray();
    }
  }
}
