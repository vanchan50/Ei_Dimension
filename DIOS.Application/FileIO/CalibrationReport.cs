using DIOS.Application.FileIO.Calibration;
using DIOS.Core.HardwareIntercom;

namespace DIOS.Application.FileIO;
public class CalibrationReport
{
  public DateTime Timestamp;
  public string MaachineName;
  public string FirmwareVersion;
  public string AppVersion;
  public float DNRCoefficient;
  public float DNRTransition;
  public ChannelConfiguration ChannelConfig;
  public bool FinalStatus;

  public CalibrationReportData GreenA;
  public CalibrationReportData GreenB;
  public CalibrationReportData GreenC;

  public CalibrationReportData RedB;
  public CalibrationReportData RedC;
  public CalibrationReportData RedD;
}