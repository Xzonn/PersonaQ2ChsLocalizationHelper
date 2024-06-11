using Mono.Options;
using PQ2Helper.Commands;
using System.Text;

namespace PQ2Helper;

public class Program
{
  public static int Main(string[] args)
  {
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    CommandSet commands = new("PersonaQ2ChsLocalizationHelper")
    {
      "Usage: PersonaQ2ChsLocalizationHelper COMMAND [OPTIONS]",
      "",
      "Available commands:",
      new ExportCommand(),
      new ImportCommand(),
    };

    return commands.Run(args);
  }
}
