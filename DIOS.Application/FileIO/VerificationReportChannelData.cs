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
    Status = CalculateStatus();
  }

  private bool CalculateStatus()
  {
    var tolerancePercent = Tolerance / 100;
    var epsilon = Target * tolerancePercent;
    if (CV <= MaxCV)
    {
      if (MFI >= Target - epsilon && MFI <= Target + epsilon)
      {
        return true;
      }
    }
    return false;
  }
}