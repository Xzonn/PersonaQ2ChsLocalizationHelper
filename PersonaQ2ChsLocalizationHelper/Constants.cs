using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace PQ2Helper;

internal class Constants
{
  public readonly static JsonSerializerOptions JSON_OPTION = new()
  {
    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    WriteIndented = true,
  };
  public readonly static Encoding ENCODING = Encoding.GetEncoding(932);
  public readonly static string[] EXTENSIONS_TO_EXPORT = [".ctd", ".ctpk", ".ftd", ".qtd", ".tbl"];
}
