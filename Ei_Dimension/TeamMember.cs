using System;
using System.ComponentModel;

namespace Ei_Dimension
{

  public class TeamMember : INotifyPropertyChanged
  {
    private string _Name;
    public string Name
    {
      get
      {
        return _Name;
      }
      set
      {
        _Name = value;
        OnPropertyChanged("Name");
      }
    }

    private double _CustomerSatScore;
    public double CustomerSatScore
    {
      get
      {
        return _CustomerSatScore;
      }
      set
      {
        _CustomerSatScore = value;
        OnPropertyChanged("CustomerSatScore");
      }
    }

    public TeamMember(string name, double customerSatScore)
    {
      _Name = name;
      _CustomerSatScore = customerSatScore;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      }
    }

  }
}