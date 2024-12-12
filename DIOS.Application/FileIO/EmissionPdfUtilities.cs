using DIOS.Application.FileIO.Verification;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Snippets.Font;
using PdfSharp.WPFonts;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using DIOS.Application.FileIO.Calibration;

namespace DIOS.Application.FileIO;

public static class EmissionPdfUtilities
{
  private static readonly List<string[]> _contents = new();

  public static void DrawVerificationHeader(this PdfSharpUtilities pdf, XFont font, VerificationReport report)
  {
    var format = XStringFormats.Center;
    _contents.Clear();
    _contents.Add([
      "Date",
      "Time",
      "Channel Configuration",
      "Event Height",
    ]);
    _contents.Add([
      report.Timestamp.AsSpan(0, 10).ToString(),
      report.Timestamp.AsSpan(11, 5).ToString(),
      report.ChannelConfig,
      report.EventHeight.ToString("F1")
    ]);
    pdf.drawTable(0, 0.6, 16.5, 1, font, format, _contents);

    _contents.Clear();
    _contents.Add([
      "DNR Coefficient",
      "DNR Transition",
      "Low Gate",
      "High Gate"
    ]);
    _contents.Add([
      report.DNRCoefficient.ToString("F1"),
      report.DNRTransition.ToString("F1"),
      report.LowGate.ToString("F1"),
      report.HighGate.ToString("F1")
    ]);
    pdf.drawTable(0, 1.7, 16.5, 1, font, format, _contents);

    _contents.Clear();
    _contents.Add([
      "Machine Name",
      "Firmware Version",
      "DIOS Version",
      "Result"
    ]);
    string machName = report.MachineName;
    if (machName.Length > 20)
    {
      machName = machName.AsSpan(0, 20).ToString();
    }

    _contents.Add([
      machName, //max 20 symb
      report.FirmwareVersion ?? "N/A",
      report.AppVersion,
      report.Status ? "Passed" : "Failed"
    ]);
    pdf.drawTable(0, 2.8, 16.5, 1, font, format, _contents);
  }

  public static void DrawVerificationChannelCount(this PdfSharpUtilities pdf, double positionY, XFont font,
    VerificationReport report)
  {
    var format = XStringFormats.Center;
    _contents.Clear();
    _contents.Add([
      "Region Count",
      "Count",
      "Min Count",
      "Result"
    ]);

    foreach (var regionData in report.regionsData)
    {
      _contents.Add([
        $"Region #{regionData.Label}",
        regionData.Count.ToString(),
        report.MinCount.ToString(),
        regionData.Count >= report.MinCount ? "Passed" : "Failed"
      ]);
    }

    var height = _contents.Count * 0.5;
    pdf.drawTable(0, positionY, 9.9, height, font, format, _contents);
  }

  public static void DrawVerificationChannelTable(this PdfSharpUtilities pdf, double positionY, XFont font,
    VerificationReport report, FieldInfo property, string label = null)
  {
    var format = XStringFormats.Center;
    _contents.Clear();
    _contents.Add([
      label ?? property.Name,
      "MFI",
      "Target",
      "Tolerance %",
      "CV",
      "Max CV",
      "Result"
    ]);
    foreach (var regionData in report.regionsData)
    {
      var t = property.GetValue(regionData) as VerificationReportChannelData;
      _contents.Add([
        $"Region #{regionData.Label}",
        t.MFI.ToString("F1"),
        t.Target.ToString("F1"),
        t.Tolerance.ToString("F1"),
        t.CV.ToString("F1"),
        t.MaxCV.ToString("F1"),
        t.Status ? "Passed" : "Failed"
      ]);
    }

    var height = _contents.Count * 0.5;
    pdf.drawTable(0, positionY, 16.5, height, font, format, _contents);
  }

  public static void DrawCalibrationHeader(this PdfSharpUtilities pdf, XFont font, CalibrationReport report)
  {
    var format = XStringFormats.Center;
    _contents.Clear();
    _contents.Add([
      "Date",
      "Time",
      "Channel Configuration"
    ]);
    _contents.Add([
      report.Timestamp.AsSpan(0, 10).ToString(),
      report.Timestamp.AsSpan(11, 5).ToString(),
      report.ChannelConfig
    ]);
    pdf.drawTable(0, 0.6, 16.5, 1, font, format, _contents);

    _contents.Clear();
    _contents.Add([
      "Machine Name",
      "Firmware Version",
      "DIOS Version"
    ]);
    string machName = report.MachineName;
    if (machName.Length > 20)
    {
      machName = machName.AsSpan(0, 20).ToString();
    }

    _contents.Add([
      machName, //max 20 symb
      report.FirmwareVersion ?? "N/A",
      report.AppVersion,
    ]);
    pdf.drawTable(0, 1.7, 16.5, 1, font, format, _contents);

    _contents.Clear();
    _contents.Add([
      "DNR Coefficient",
      "DNR Transition",
      "Result"
    ]);
    _contents.Add([
      report.DNRCoefficient.ToString("F1"),
      report.DNRTransition.ToString("F1"),
      report.Status ? "Passed" : "Failed"
    ]);
    pdf.drawTable(0, 2.8, 16.5, 1, font, format, _contents);
  }

  public static void DrawCalibrationChannelTable(this PdfSharpUtilities pdf, double positionY, XFont font, CalibrationReportData channelData)
  {
    var format = XStringFormats.Center;
    _contents.Clear();
    _contents.Add([
      channelData.Label,
      channelData.Temperature.ToString("F1"),
      channelData.Bias30.ToString(),
      channelData.MFI.ToString("F1"),
      channelData.CV.ToString("F1"),
      channelData.Target.ToString(),
      channelData.Margin.ToString("F1")
    ]);
    pdf.drawTable(0, positionY, 16.5, 0.5, font, format, _contents);
  }
}

public class DPoint
{
  public double x { get; set; }
  public double y { get; set; }

  public DPoint(double x, double y)
  {
    this.x = x;
    this.y = y;
  }
}

public class Interpolation
{
  public double linearInterpolation(double x0, double y0, double x1, double y1, double xd)
  {
    /*******************************************************************/
    //
    //  x0          ------->    y0
    //  given x     ------->    what is y?
    //  x1          ------->    y1
    /*******************************************************************/

    return (y0 + ((y1 - y0) * ((xd - x0) / (x1 - x0))));
  }
}

public class CustomFontResolver : IFontResolver
{
  /// <summary>
  /// Identifies a font.
  /// </summary>
  private readonly record struct FontKey(
      string FamilyName,
      bool IsBold,
      bool IsItalic)
  {
    public string GetFaceName() =>
        (string.IsNullOrEmpty(FamilyName) ? "Empty" : FamilyName) +
        (IsBold ? "-Bold" : "") +
        (IsItalic ? "-Italic" : "");
  };

  /// <summary>
  /// Represents information associated with a font.
  /// </summary>
  private class FontMeta
  {
    public string FileName { get; init; }
  }

  /// <summary>
  /// Specifies how to search for the font.
  /// </summary>
  private static readonly EnumerationOptions FontSearchOptions = new()
  {
    RecurseSubdirectories = true,
    MatchCasing = MatchCasing.CaseInsensitive,
    AttributesToSkip = 0,
    IgnoreInaccessible = true
  };


  private readonly SegoeWpFontResolver fallbackFontResolver;
  private readonly Dictionary<string, FontMeta> fontsByFace;


  /// <summary>
  /// Initializes a new instance of the class.
  /// </summary>
  public CustomFontResolver()
  {
    fallbackFontResolver = new SegoeWpFontResolver();
    fontsByFace = new Dictionary<string, FontMeta>();
  }


  /// <summary>
  /// Finds the specified font in the file system.
  /// </summary>
  private static FontResolverInfo FindFont(FontKey fontKey, out string fileName)
  {
    if (string.IsNullOrEmpty(fontKey.FamilyName))
    {
      fileName = "";
      return null;
    }

    try
    {
      ICollection<string> fontDirectories = GetFontDirectories();
      ICollection<string> desiredNames = GetDesiredNames(fontKey);
      ICollection<string> candidateNames = GetCandidateNames(fontKey);
      FileInfo candidateFileInfo = null;

      foreach (string fontDirectory in fontDirectories)
      {
        if (!Directory.Exists(fontDirectory))
          continue;

        foreach (FileInfo fileInfo in new DirectoryInfo(fontDirectory)
            .EnumerateFiles(fontKey.FamilyName + "*.ttf", FontSearchOptions))
        {
          if (desiredNames.Any(name => name.Equals(fileInfo.Name, StringComparison.OrdinalIgnoreCase)))
          {
            fileName = fileInfo.FullName;
            return new FontResolverInfo(fontKey.GetFaceName(), false, false);
          }

          if (candidateFileInfo == null &&
              candidateNames.Any(name => name.Equals(fileInfo.Name, StringComparison.OrdinalIgnoreCase)))
          {
            candidateFileInfo = fileInfo;
          }
        }
      }

      if (candidateFileInfo != null)
      {
        fileName = candidateFileInfo.FullName;
        return new FontResolverInfo(fontKey.GetFaceName(), fontKey.IsBold, fontKey.IsItalic);
      }
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex);
    }

    fileName = "";
    return null;
  }

  /// <summary>
  /// Gets the font directories depending on the OS.
  /// </summary>
  private static ICollection<string> GetFontDirectories()
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
      return new string[]
      {
                  @"C:\Windows\Fonts\",
                  Path.Combine(
                      Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                      @"Microsoft\Windows\Fonts")
      };
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
      return new string[]
      {
                  "/usr/share/fonts/truetype/"
      };
    }
    else
    {
      return Array.Empty<string>();
    }
  }

  /// <summary>
  /// Gets desired font file names.
  /// </summary>
  private static ICollection<string> GetDesiredNames(FontKey fontKey)
  {
    if (fontKey.IsBold && fontKey.IsItalic)
    {
      return new string[]
      {
                  fontKey.FamilyName + "bi.ttf",
                  fontKey.FamilyName + "-BoldItalic.ttf",
                  fontKey.FamilyName + "-BoldOblique.ttf"
      };
    }
    else if (fontKey.IsBold)
    {
      return new string[]
      {
                  fontKey.FamilyName + "bd.ttf",
                  fontKey.FamilyName + "-Bold.ttf"
      };
    }
    else if (fontKey.IsItalic)
    {
      return new string[]
      {
                  fontKey.FamilyName + "i.ttf",
                  fontKey.FamilyName + "-Italic.ttf",
                  fontKey.FamilyName + "-Oblique.ttf"
      };
    }
    else
    {
      return new string[]
      {
                  fontKey.FamilyName + ".ttf",
                  fontKey.FamilyName + "-Regular.ttf"
      };
    }
  }

  /// <summary>
  /// Gets possible font file names.
  /// </summary>
  private static ICollection<string> GetCandidateNames(FontKey fontKey)
  {
    if (fontKey.IsBold || fontKey.IsItalic)
    {
      return new string[]
      {
                  fontKey.FamilyName + ".ttf",
                  fontKey.FamilyName + "-Regular.ttf"
      };
    }
    else
    {
      return Array.Empty<string>();
    }
  }


  /// <summary>
  /// Converts specified information about a required typeface into a specific font.
  /// </summary>
  /// <remarks>
  /// PDFsharp calls ResolveTypeface only once for each unique combination of familyName, isBold, and isItalic.
  /// </remarks>
  public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
  {
    FontKey fontKey = new(familyName, isBold, isItalic);
    FontResolverInfo resolverInfo =
        FindFont(fontKey, out string fileName) ??
        fallbackFontResolver.ResolveTypeface(isBold
            ? SegoeWpFontResolver.FamilyNames.SegoeWPBold
            : SegoeWpFontResolver.FamilyNames.SegoeWP,
            false, isItalic) ??
        new FontResolverInfo(fontKey.GetFaceName(), isBold, isItalic);

    fontsByFace[resolverInfo.FaceName] = new FontMeta { FileName = fileName };
    return resolverInfo;
  }

  /// <summary>
  /// Gets the bytes of a physical font with specified face name.
  /// </summary>
  /// <remarks>
  /// A face name previously retrieved by ResolveTypeface.
  /// PDFsharp never calls GetFont twice with the same face name.
  /// </remarks>
  public byte[] GetFont(string faceName)
  {
    try
    {
      if (fontsByFace.TryGetValue(faceName, out FontMeta fontMeta))
      {
        byte[] font = string.IsNullOrEmpty(fontMeta.FileName)
            ? fallbackFontResolver.GetFont(faceName)
            : File.ReadAllBytes(fontMeta.FileName);

        if (font != null)
          return font;
      }
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex);
    }

    return FontDataHelper.SegoeWP;
  }
}