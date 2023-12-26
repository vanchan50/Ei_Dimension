namespace DIOS.Application.FileIO.Verification;
public class VerificationReportRegionData
{
  public readonly VerificationReportChannelData GreenSSC;
  public readonly VerificationReportChannelData RedSSC;
  public readonly VerificationReportChannelData Cl1;
  public readonly VerificationReportChannelData CL2;
  public readonly VerificationReportChannelData Reporter;
  public readonly int Count;
  public readonly bool Status;
  public readonly string Label;

  public VerificationReportRegionData(string label, VerificationReportChannelData greenSsc, VerificationReportChannelData redSsc, VerificationReportChannelData cl1, VerificationReportChannelData cl2, VerificationReportChannelData reporter, int count)
  {
    GreenSSC = greenSsc;
    RedSSC = redSsc;
    Cl1 = cl1;
    CL2 = cl2;
    Reporter = reporter;
    Status = greenSsc.Status && redSsc.Status && cl1.Status && cl2.Status && reporter.Status;
    Label = label;
    Count = count;
  }
}