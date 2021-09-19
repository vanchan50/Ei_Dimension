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

    private readonly Dictionary<WellType, string> _stateDict;

    public WellTableRow(int rowIndex, int size)
    {
      _stateDict = new Dictionary<WellType, string>();
      _stateDict.Add(WellType.Empty, @"/Icons/Empty_Well.png");
      _stateDict.Add(WellType.Standard, @"pack://application:,,,/DevExpress.Images.v21.1;component/Images/Actions/Apply_32x32.png");
      _stateDict.Add(WellType.Control, @"pack://application:,,,/DevExpress.Images.v21.1;component/Images/Media/Media_32x32.png");
      _stateDict.Add(WellType.Unknown, @"pack://application:,,,/DevExpress.Images.v21.1;component/Images/Find/Find_32x32.png");

      Index = rowIndex;
      Header = Convert.ToChar('A' + Index).ToString();
      Images = new ObservableCollection<string>();
      Types = new List<WellType>(size);
      for (var i = 0; i < size; i++)
      {
        Images.Add(_stateDict[WellType.Empty]);
        Types.Add(WellType.Empty);
      }
    }

    public void SetType(int index, WellType type)
    {
      Types[index] = type;
      Images[index] = _stateDict[Types[index]];
    }
  }
}
