using DIOS.Application.FileIO.Verification;

namespace DIOS.Application.FileIO;

public class VerificationReport
{
  public readonly string Timestamp = DateTime.Now.ToString("dd.MM.yyyy.HH-mm-ss", System.Globalization.CultureInfo.CreateSpecificCulture("en-GB"));
  public readonly string MachineName = Environment.MachineName;
  public readonly string FirmwareVersion;
  public readonly string AppVersion;
  public readonly float DNRCoefficient;
  public readonly float DNRTransition;
  public readonly float EventHeight;
  public readonly float LowGate;
  public readonly float HighGate;
  public readonly string ChannelConfig;
  public readonly int MinCount;
  public bool Status
  {
    get
    {
      var result = true;
      foreach (var regionData in regionsData)
      {
        result = result && regionData.Status &&
                 regionData.Count >= MinCount;
      }
      return result;
    }
  }

  public readonly List<VerificationReportRegionData> regionsData = new(4);

  public VerificationReport(string firmwareVersion, string appVersion,
    float dnrCoefficient, float dnrTransition, string channelConfig,
    float eventHeight, float lowGate, float highGate, int minCount)
  {
    FirmwareVersion = firmwareVersion;
    AppVersion = appVersion;
    DNRCoefficient = dnrCoefficient;
    DNRTransition = dnrTransition;
    ChannelConfig = channelConfig;
    EventHeight = eventHeight;
    LowGate = lowGate;
    HighGate = highGate;
    MinCount = minCount;
  }
}