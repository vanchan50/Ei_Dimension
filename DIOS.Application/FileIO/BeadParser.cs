using DIOS.Core;

namespace DIOS.Application.FileIO;

public class BeadParser
{
  private static readonly char[] Separator = { ',' };
  private static string[] _wordsbuffer = new string[25];

  public static bool ParseBeadInfoFile(string path, List<ProcessedBead> outputList)
  {
    List<string> linesInFile = ParseFileToBeadStrings(path);
    if (linesInFile.Count == 1 && linesInFile[0] == " ")
    {
      return false;
    }

    for (var i = 0; i < linesInFile.Count; i++)
    {
      try
      {
        var bs = ParseRow(linesInFile[i]);
        outputList.Add(bs);
      }
      catch (FormatException) { }
    }
    return true;
  }

  private static List<string> ParseFileToBeadStrings(string path)
  {
    var str = new List<string>(2000000);
    using (var fin = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
      using (var sr = new StreamReader(fin))
      {
        // ReSharper disable once MethodHasAsyncOverload
        sr.ReadLine();
        while (!sr.EndOfStream)
        {
          str.Add(sr.ReadLine());
        }
      }
    return str;
  }

  private static ProcessedBead ParseRow(string data)
  {
    var numFormat = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
    _wordsbuffer = data.Split(Separator);
    int i = 0;
    ProcessedBead binfo = new ProcessedBead
    {
      EventTime = uint.Parse(_wordsbuffer[i++]),
      fsc_bg = byte.Parse(_wordsbuffer[i++]),
      vssc_bg = byte.Parse(_wordsbuffer[i++]),
      greenD_bg = byte.Parse(_wordsbuffer[i++]),
      cl1_bg = ushort.Parse(_wordsbuffer[i++]),
      cl2_bg = ushort.Parse(_wordsbuffer[i++]),
      redA_bg = byte.Parse(_wordsbuffer[i++]),
      rssc_bg = byte.Parse(_wordsbuffer[i++]),
      gssc_bg = byte.Parse(_wordsbuffer[i++]),
      greenB_bg = ushort.Parse(_wordsbuffer[i++]),
      greenC_bg = ushort.Parse(_wordsbuffer[i++]),
      greenB = float.Parse(_wordsbuffer[i++]),
      greenC = float.Parse(_wordsbuffer[i++]),
      l_offset_rg = byte.Parse(_wordsbuffer[i++]),
      l_offset_gv = byte.Parse(_wordsbuffer[i++]),

      region = (ushort.Parse(_wordsbuffer[i])) % ProcessedBead.ZONEOFFSET, //16123
      zone = (ushort.Parse(_wordsbuffer[i++])) / ProcessedBead.ZONEOFFSET,

      ratio1 = float.Parse(_wordsbuffer[i++], numFormat),
      ratio2 = float.Parse(_wordsbuffer[i++], numFormat),
      greenD = float.Parse(_wordsbuffer[i++], numFormat),
      redssc = float.Parse(_wordsbuffer[i++], numFormat),
      cl1 = float.Parse(_wordsbuffer[i++], numFormat),
      cl2 = float.Parse(_wordsbuffer[i++], numFormat),
      redA = float.Parse(_wordsbuffer[i++], numFormat),
      greenssc = float.Parse(_wordsbuffer[i++], numFormat),
      reporter = float.Parse(_wordsbuffer[i], numFormat),
    };
    return binfo;
  }
}