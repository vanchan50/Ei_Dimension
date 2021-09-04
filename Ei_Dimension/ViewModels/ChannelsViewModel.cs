using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ChannelsViewModel
  {
    public virtual ObservableCollection<string> Bias30Parameters { get; set; }
    public virtual ObservableCollection<string> TcompBiasParameters { get; set; }
    public virtual ObservableCollection<string> TempParameters { get; set; }
    public virtual ObservableCollection<string> BackgroundParameters { get; set; }
    public virtual ObservableCollection<string> SiPMTempCoeff { get; set; }
    public virtual ObservableCollection<string> CurrentMapName { get; set; }

    public static ChannelsViewModel Instance { get; private set; }

    protected ChannelsViewModel()
    {
      CurrentMapName = new ObservableCollection<string> { App.Device.ActiveMap.mapName };

      Bias30Parameters = new ObservableCollection<string>();
      Bias30Parameters.Add(App.Device.ActiveMap.calgssc.ToString());
      Bias30Parameters.Add(App.Device.ActiveMap.calrpmaj.ToString());
      Bias30Parameters.Add(App.Device.ActiveMap.calrpmin.ToString());
      Bias30Parameters.Add("");
      Bias30Parameters.Add(App.Device.ActiveMap.calrssc.ToString());
      Bias30Parameters.Add(App.Device.ActiveMap.calcl1.ToString());
      Bias30Parameters.Add(App.Device.ActiveMap.calcl2.ToString());
      Bias30Parameters.Add(App.Device.ActiveMap.calvssc.ToString());
      Bias30Parameters.Add("");
      Bias30Parameters.Add("");
      if (App.Device.ActiveMap.dimension3)
      {
        Bias30Parameters[3] = App.Device.ActiveMap.calcl3.ToString();
        Bias30Parameters[8] = App.Device.ActiveMap.calcl0.ToString();
      }
      TcompBiasParameters = new ObservableCollection<string>();
      TempParameters = new ObservableCollection<string>();
      BackgroundParameters = new ObservableCollection<string>();
      for(var i = 0; i < 10; i++)
      {
        TcompBiasParameters.Add("");
        TempParameters.Add("");
        BackgroundParameters.Add("");
      }
      SiPMTempCoeff = new ObservableCollection<string> { "" };
      Instance = this;
    }

    public static ChannelsViewModel Create()
    {
      return ViewModelSource.Create(() => new ChannelsViewModel());
    }

    public void UpdateBiasButtonClick()
    {

    }

    public void SaveBiasButtonClick()
    {

    }
    
    public void FocusedBox(int num)
    {
      switch (num)
      {
        case 0:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 0);
          break;
        case 1:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 1);
          break;
        case 2:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 2);
          break;
        case 3:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 3);
          break;
        case 4:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 4);
          break;
        case 5:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 5);
          break;
        case 6:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 6);
          break;
        case 7:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 7);
          break;
        case 8:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 8);
          break;
        case 9:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 9);
          break;
        case 10:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(TcompBiasParameters)), this, 0);
          break;
        case 11:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(TcompBiasParameters)), this, 1);
          break;
        case 12:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(TcompBiasParameters)), this, 2);
          break;
        case 13:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(TcompBiasParameters)), this, 3);
          break;
        case 14:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(TcompBiasParameters)), this, 4);
          break;
        case 15:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(TcompBiasParameters)), this, 5);
          break;
        case 16:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(TcompBiasParameters)), this, 6);
          break;
        case 17:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(TcompBiasParameters)), this, 7);
          break;
        case 18:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(TcompBiasParameters)), this, 8);
          break;
        case 19:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(TcompBiasParameters)), this, 9);
          break;
        case 20:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(TempParameters)), this, 0);
          break;
        case 21:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(TempParameters)), this, 1);
          break;
        case 22:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(TempParameters)), this, 2);
          break;
        case 23:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(TempParameters)), this, 3);
          break;
        case 24:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(TempParameters)), this, 4);
          break;
        case 25:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(TempParameters)), this, 5);
          break;
        case 26:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(TempParameters)), this, 6);
          break;
        case 27:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(TempParameters)), this, 7);
          break;
        case 28:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(TempParameters)), this, 8);
          break;
        case 29:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(TempParameters)), this, 9);
          break;
        case 30:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(BackgroundParameters)), this, 0);
          break;
        case 31:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(BackgroundParameters)), this, 1);
          break;
        case 32:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(BackgroundParameters)), this, 2);
          break;
        case 33:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(BackgroundParameters)), this, 3);
          break;
        case 34:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(BackgroundParameters)), this, 4);
          break;
        case 35:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(BackgroundParameters)), this, 5);
          break;
        case 36:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(BackgroundParameters)), this, 6);
          break;
        case 37:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(BackgroundParameters)), this, 7);
          break;
        case 38:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(BackgroundParameters)), this, 8);
          break;
        case 39:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(BackgroundParameters)), this, 9);
          break;
        case 40:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(SiPMTempCoeff)), this, 0);
          break;
      }
    }

  }
}