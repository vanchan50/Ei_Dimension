using DIOS.Core;
using System.Collections.Generic;
using System.IO;
using System;

namespace DIOS.Application.FileIO
{
  public class BeadParser
  {
    private static readonly char[] Separator = { ',' };

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
      string[] words = data.Split(Separator);
      int i = 0;
      ProcessedBead binfo = new ProcessedBead
      {
        EventTime = uint.Parse(words[i++]),
        fsc_bg = byte.Parse(words[i++]),
        vssc_bg = byte.Parse(words[i++]),
        cl0_bg = byte.Parse(words[i++]),
        cl1_bg = ushort.Parse(words[i++]),
        cl2_bg = ushort.Parse(words[i++]),
        cl3_bg = byte.Parse(words[i++]),
        rssc_bg = byte.Parse(words[i++]),
        gssc_bg = byte.Parse(words[i++]),
        greenB_bg = ushort.Parse(words[i++]),
        greenC_bg = ushort.Parse(words[i++]),
        greenB = float.Parse(words[i++]),
        greenC = float.Parse(words[i++]),
        l_offset_rg = byte.Parse(words[i++]),
        l_offset_gv = byte.Parse(words[i++]),

        region = (ushort.Parse(words[i])) % ProcessedBead.ZONEOFFSET, //16123
        zone = (ushort.Parse(words[i++])) / ProcessedBead.ZONEOFFSET,

        fsc = float.Parse(words[i++], numFormat),
        violetssc = float.Parse(words[i++], numFormat),
        cl0 = float.Parse(words[i++], numFormat),
        redssc = float.Parse(words[i++], numFormat),
        cl1 = float.Parse(words[i++], numFormat),
        cl2 = float.Parse(words[i++], numFormat),
        cl3 = float.Parse(words[i++], numFormat),
        greenssc = float.Parse(words[i++], numFormat),
        reporter = float.Parse(words[i], numFormat),
      };
      return binfo;
    }
  }
}
