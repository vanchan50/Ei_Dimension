using System.Drawing;
using PdfSharp.Drawing;

namespace DIOS.Application.FileIO
{
  public class CalibrationReportPdfFileWriter
  {
    private CalibrationReport _report;

    public CalibrationReportPdfFileWriter(CalibrationReport report)
    {
      _report = report;
    }

    public void CreateAndSaveVerificationPdf(string path)
    {
      PdfSharpUtilities pdf = new PdfSharpUtilities(path, "Calibration Report");
      XFont font = new XFont(new Font("Segoe UI Semilight", 11).Name, 11);

      pdf.addText("Calibration Report", font, new DPoint(0, 0.1));

      font = new XFont(new Font("Segoe UI Semilight", 10).Name, 10);

      pdf.DrawCalibrationHeader(font, _report);

      if (_report.channelsData.Count > 0)
      {
        var calTableYPos = 4.8;
        var format = XStringFormats.Center;
        List<string[]> contents =
        [[
          "Channel",
          "Temp",
          "Bias @ 30",
          "MFI",
          "CV",
          "Target",
          "Margin"
        ]];
        pdf.drawTable(0, calTableYPos, 16.5, 0.5, font, format, contents);
        calTableYPos += 0.5;
        foreach (var channel in _report.channelsData)
        {
          pdf.DrawCalibrationChannelTable(calTableYPos, font, channel);
          calTableYPos += 0.5;
        }
      }

      //var uri = new Uri(@"/Ei_Dimension;component/Icons/Emission_Logo.png", UriKind.Relative);
      //System.Reflection.Assembly thisExe = System.Reflection.Assembly.GetExecutingAssembly();
      //Stream file =
      //  thisExe.GetManifestResourceStream(uri.ToString());
      //XImage yourImage = XImage.FromStream(imgStream);
      pdf.Save();
    }
  }
}
