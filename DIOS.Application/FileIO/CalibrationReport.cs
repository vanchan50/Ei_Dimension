using DIOS.Application.FileIO.Calibration;

namespace DIOS.Application.FileIO;
public class CalibrationReport
{
  public readonly string Timestamp = DateTime.Now.ToString("dd.MM.yyyy.hh-mm-ss", System.Globalization.CultureInfo.CreateSpecificCulture("en-GB"));
  public readonly string MachineName = Environment.MachineName;
  public readonly string FirmwareVersion;
  public readonly string AppVersion;
  public readonly float DNRCoefficient;
  public readonly float DNRTransition;
  public readonly string ChannelConfig;
  public readonly bool Status;
  public readonly List<CalibrationReportData> channelsData = new(5);//Green ABC, Red BCD.

  public CalibrationReport(string firmwareVersion, string appVersion, float dnrCoefficient, float dnrTransition, string channelConfig, bool status)
  {
    FirmwareVersion = firmwareVersion;
    AppVersion = appVersion;
    DNRCoefficient = dnrCoefficient;
    DNRTransition = dnrTransition;
    ChannelConfig = channelConfig;
    Status = status;
  }
}