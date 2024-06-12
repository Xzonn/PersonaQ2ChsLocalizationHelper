using PersonaEditorLib.FileContainer;
using PersonaEditorLib.Text;
using PersonaEditorLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PQ2Helper;

internal class Helper
{
  public static void EnumerateFiles(string inputFolder, Action<GameFile, string> action)
  {
    foreach (var filePath in Directory.EnumerateFiles(inputFolder, "*", SearchOption.AllDirectories))
    {
      var sheetName = Path.GetRelativePath(inputFolder, filePath).Replace('\\', '/');
      var extension = Path.GetExtension(filePath).ToLowerInvariant();
      IGameData gameData;
      switch (extension)
      {
        case ".bin":
        case ".arc":
        case ".pack":
        case ".tpc":
          gameData = new BIN(File.ReadAllBytes(filePath));
          break;
        case ".pm1":
          gameData = new PM1(File.ReadAllBytes(filePath));
          break;
        case ".bvp":
          gameData = new BVP("", File.ReadAllBytes(filePath));
          break;
        case ".bf":
          gameData = new BF(File.ReadAllBytes(filePath), "");
          break;
        case ".bmd":
          gameData = new BMD(File.ReadAllBytes(filePath));
          break;
        default:
          continue;
      }
      action(new GameFile(sheetName, gameData), sheetName);
    }
  }
}
