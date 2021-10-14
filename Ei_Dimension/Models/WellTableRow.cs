using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ei_Dimension.Models
{
  public class WellTableRow
  {
    public int Index { get; }
    public string Header { get; }
    public ObservableCollection<string> Images { get; set; }
    public List<WellType> Types { get; set; }

    public WellTableRow(int rowIndex, int size)
    {
      Index = rowIndex;
      Header = Convert.ToChar('A' + Index).ToString();
      Images = new ObservableCollection<string>();
      Types = new List<WellType>(size);
      for (var i = 0; i < size; i++)
      {
        Images.Add(Image(WellType.Empty));
        Types.Add(WellType.Empty);
      }
    }

    public void SetType(int index, WellType type)
    {
      Types[index] = type;
      Images[index] = Image(type);
    }
    
    private static string Image(WellType type)
    {
      string ret = "";
      switch (type)
      {
        case WellType.Empty:
          ret = @"/Icons/Empty_Well.png";
          break;
        case WellType.Standard:
          ret = @"/Icons/Filled_Well.png";
          break;
        case WellType.Control:
          ret = @"pack://application:,,,/DevExpress.Images.v21.1;component/Images/Media/Media_32x32.png";
          break;
        case WellType.Unknown:
          ret = @"pack://application:,,,/DevExpress.Images.v21.1;component/Images/Find/Find_32x32.png";
          break;
        case WellType.NowReading:
          ret = @"pack://application:,,,/DevExpress.Images.v21.1;component/SvgImages/Outlook Inspired/GettingStarted.svg";
          break;
        case WellType.Success:
          ret = @"pack://application:,,,/DevExpress.Images.v21.1;component/SvgImages/Icon Builder/Actions_CheckCircled.svg";
          break;
      }
      return ret;
    }
  }
}
