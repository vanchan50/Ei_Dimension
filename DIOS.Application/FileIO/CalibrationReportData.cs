namespace DIOS.Application.FileIO.Calibration;
public class CalibrationReportData
{
  public readonly float Temperature;//1decimal format
  public readonly float MFI;
  public readonly float CV;
  public readonly float Margin;
  public readonly int Target;
  public readonly int Bias30;
  public readonly string Label;

  public CalibrationReportData(string label, float temperature, int bias, int target, DistributionStats stats)
  {
    Label = label;
    Temperature = temperature;
    Bias30 = bias;
    MFI = stats.Mean;
    CV = stats.CoeffVar;
    Target = target;
    Margin = Math.Abs(Target - MFI) / Target;
  }
}