using System.Management;

namespace DIOS.Application.SerialIO
{
  public static class ComDeviceSeeker
  {
    public static string? FindCOMDevice(string VID, string PID)
    {
      ManagementObjectCollection objCollection = null;
      try
      {
        using (ManagementObjectSearcher searcher =
               new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity"))
        {
          objCollection = searcher.Get();
        }

        foreach (var queryObj in objCollection)
        {
          if (queryObj is null)
          {
            continue;
          }

          if (queryObj["Caption"] is null)
          {
            continue;
          }

          string Caption = queryObj["Caption"].ToString();

          if (!Caption.Contains("(COM"))
          {
            continue;
          }

          if (queryObj["deviceid"] is null)
          {
            continue;
          }

          var deviceId = queryObj["deviceid"].ToString(); //"DeviceID"

          var localVID = GetVID(deviceId);
          var localPID = GetPID(deviceId);

          if (localVID == VID && localPID == PID)
          {
            var CaptionInfo = GetCaptionInfo(Caption);
            return CaptionInfo;
          }
        }
      }
      catch (ManagementException e)
      {
        throw new ComDeviceSeekerException(e.Message);
      }
      finally
      {
        objCollection?.Dispose();
      }

      return null;
    }

    private static string GetCaptionInfo(string caption)
    {
      int captionIndex = caption.IndexOf("(COM", StringComparison.InvariantCulture);
      return caption.Substring(captionIndex + 1).TrimEnd(')'); // make the trimming more correct 
    }

    private static string GetVID(string deviceID)
    {
      int vidIndex = deviceID.IndexOf("VID_", StringComparison.InvariantCulture);
      if (vidIndex == -1)
      {
        return null;
      }
      var startingAtVid = deviceID.Substring(vidIndex + 4); // + 4 to remove "VID_"                    
      return startingAtVid.Substring(0, 4); // vid is four characters long
    }

    private static string GetPID(string deviceID)
    {
      int pidIndex = deviceID.IndexOf("PID_", StringComparison.InvariantCulture);
      if (pidIndex == -1)
      {
        return null;
      }
      var startingAtPid = deviceID.Substring(pidIndex + 4); // + 4 to remove "PID_"                    
      return startingAtPid.Substring(0, 4); // pid is four characters long
    }
  }

  public class ComDeviceSeekerException : Exception
  {
    public ComDeviceSeekerException(string? message) : base(message)
    {

    }
    public ComDeviceSeekerException(string? message, Exception? innerException) : base(message, innerException)
    {
      
    }
  }
}
