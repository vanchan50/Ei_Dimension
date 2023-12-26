namespace DIOS.Application.FileIO.Verification;
public class VerificationReportChannelData
{
  public readonly float MFI;
  public readonly float CV;
  public readonly float MaxCV;
  public readonly float Tolerance;
  public readonly float Target;
  public readonly bool Status;

  public VerificationReportChannelData(float mfi, float cv, float maxCv, float tolerance, float target)
  {
    MFI = mfi;
    CV = cv;
    MaxCV = maxCv;
    Tolerance = tolerance;
    Target = target;

    var tolerancePercent = tolerance/100;
    var epsilon = Target * tolerancePercent;
    Status = false;
    if (CV <= MaxCV)
    {
      if (MFI >= Target - epsilon && MFI <= Target + epsilon)
      {
        Status = true;
      }
    }
  }
}