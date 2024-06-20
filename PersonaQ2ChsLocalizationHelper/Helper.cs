﻿using PersonaEditorLib;
using PersonaEditorLib.FileContainer;
using PersonaEditorLib.SpriteContainer;
using PersonaEditorLib.Text;
using PQ2Helper.Models;

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
      try
      {
        switch (extension)
        {
          case ".arc":
          case ".bin":
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
          case ".ctpk":
            gameData = new CTPK(filePath);
            break;
          default:
            continue;
        }
      }
      catch { continue; }
      action(new GameFile(sheetName, gameData), sheetName);
    }
  }
}
