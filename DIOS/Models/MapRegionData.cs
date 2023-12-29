using System.Collections.ObjectModel;
using Ei_Dimension.Core;

namespace Ei_Dimension.Models;

public class MapRegionData : ObservableObject
{
  public int Number { get; }
  public string NumberString { get; }
  //Are observableCollection[0] to comply with the SanityCheck mechanism
  public ObservableCollection<string> Name
  {
    get { return _name; }
    set
    {
      _name = value;
      OnPropertyChanged();
    }
  }

  public ObservableCollection<string> MFIValue
  {
    get { return _mfiValue; }
    set
    {
      _mfiValue = value;
      OnPropertyChanged();
    }
  }

  private ObservableCollection<string> _name;

  private ObservableCollection<string> _targetReporterValue;
  private ObservableCollection<string> _mfiValue;
  public MapRegionData(int number)
  {
    Number = number;
    NumberString = number.ToString();
    _name = new ObservableCollection<string> { NumberString };
    _targetReporterValue = new ObservableCollection<string> { "" };
    _mfiValue = new ObservableCollection<string> { "" };
  }
}