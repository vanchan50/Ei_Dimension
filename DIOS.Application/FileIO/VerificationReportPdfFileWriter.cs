using PdfSharp.Drawing;
using System.Drawing;
using DIOS.Application.FileIO.Verification;

namespace DIOS.Application.FileIO;

public class VerificationReportPdfFileWriter
{
  private VerificationReport _report;

  public VerificationReportPdfFileWriter(VerificationReport report)
  {
    _report = report;
  }

  public void CreateAndSaveVerificationPdf(string path)
  {
    PdfSharpUtilities pdf = new PdfSharpUtilities(path);
    XFont font = new XFont(new Font("Segoe UI Semilight", 11).Name, 11);

    pdf.addText("Verification Report", font, new DPoint(0, 0.1));

    font = new XFont(new Font("Segoe UI Semilight", 10).Name, 10);

    pdf.DrawVerificationHeader(font, _report);

    var type = typeof(VerificationReportRegionData);
    var greenSsc = type.GetField(nameof(VerificationReportRegionData.GreenSSC));
    var redSsc = type.GetField(nameof(VerificationReportRegionData.RedSSC));
    var cl1 = type.GetField(nameof(VerificationReportRegionData.CL1));
    var cl2 = type.GetField(nameof(VerificationReportRegionData.CL2));
    var reporter = type.GetField(nameof(VerificationReportRegionData.Reporter));

    pdf.DrawVerificationChannelTable( 4.8, font, _report, greenSsc, "Green SSC");
    pdf.DrawVerificationChannelTable( 8.9, font, _report, redSsc, "Red SSC");
    pdf.DrawVerificationChannelTable(13.0, font, _report, cl1);
    pdf.DrawVerificationChannelTable(17.1, font, _report, cl2);
    pdf.DrawVerificationChannelTable(21.2, font, _report, reporter);

    //var uri = new Uri(@"/Ei_Dimension;component/Icons/Emission_Logo.png", UriKind.Relative);
    //System.Reflection.Assembly thisExe = System.Reflection.Assembly.GetExecutingAssembly();
    //Stream file =
    //  thisExe.GetManifestResourceStream(uri.ToString());
    //XImage yourImage = XImage.FromStream(imgStream);
    pdf.Save();
  }
}