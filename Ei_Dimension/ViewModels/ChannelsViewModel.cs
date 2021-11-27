using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Windows.Controls;

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
    public virtual string SelectedSensitivityContent { get; set; }
    public virtual ObservableCollection<DropDownButtonContents> SensitivityItems { get; set; }

    public static ChannelsViewModel Instance { get; private set; }
    public byte SelectedSensitivityIndex { get; set; }

    protected ChannelsViewModel()
    {
      CurrentMapName = new ObservableCollection<string> { App.Device.ActiveMap.mapName };

      Bias30Parameters = new ObservableCollection<string>();
      Bias30Parameters.Add(App.Device.ActiveMap.calgssc.ToString());
      Bias30Parameters.Add(App.Device.ActiveMap.calrpmaj.ToString());
      Bias30Parameters.Add(App.Device.ActiveMap.calrpmin.ToString());
      Bias30Parameters.Add(App.Device.ActiveMap.calcl3.ToString());
      Bias30Parameters.Add(App.Device.ActiveMap.calrssc.ToString());
      Bias30Parameters.Add(App.Device.ActiveMap.calcl1.ToString());
      Bias30Parameters.Add(App.Device.ActiveMap.calcl2.ToString());
      Bias30Parameters.Add(App.Device.ActiveMap.calvssc.ToString());
      Bias30Parameters.Add(App.Device.ActiveMap.calcl0.ToString());
      Bias30Parameters.Add(App.Device.ActiveMap.calfsc.ToString());
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

      var RM = Language.Resources.ResourceManager;
      var curCulture = Language.TranslationSource.Instance.CurrentCulture;
      SensitivityItems = new ObservableCollection<DropDownButtonContents>
      {
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Channels_Sens_B), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Channels_Sens_C), curCulture), this)
      };
      SelectedSensitivityIndex = Settings.Default.SensitivityChannelB? (byte)0 : (byte)1;
      SelectedSensitivityContent = SensitivityItems[SelectedSensitivityIndex].Content;
      Instance = this;
    }

    public static ChannelsViewModel Create()
    {
      return ViewModelSource.Create(() => new ChannelsViewModel());
    }

    public void UpdateBiasButtonClick()
    {
      App.Device.MainCommand("RefreshDac");
      App.Device.InitSTab("channeltab");
    }

    public void SaveBiasButtonClick()
    {
      App.Device.SaveCalVals(new MicroCy.MapCalParameters
      {
        TempCl0 = int.Parse(Bias30Parameters[8]),
        TempCl1 = int.Parse(Bias30Parameters[5]),
        TempCl2 = int.Parse(Bias30Parameters[6]),
        TempCl3 = int.Parse(Bias30Parameters[3]),
        TempRedSsc = int.Parse(Bias30Parameters[4]),
        TempGreenSsc = int.Parse(Bias30Parameters[0]),
        TempVioletSsc = int.Parse(Bias30Parameters[7]),
        TempRpMaj = int.Parse(Bias30Parameters[1]),
        TempRpMin = int.Parse(Bias30Parameters[2]),
        TempFsc = int.Parse(Bias30Parameters[9]),
        MinSSC = ushort.Parse(CalibrationViewModel.Instance.EventTriggerContents[1]),
        MaxSSC = ushort.Parse(CalibrationViewModel.Instance.EventTriggerContents[2]),
        Caldate = null,
        Valdate = null
      });
    }

    public void SetOffsetButtonClick()
    {
      App.Device.MainCommand("SetBaseline");
    }

    public void FocusedBox(int num)
    {
      switch (num)
      {
        case 0:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 0);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelsView.Instance.SP.Children[0]);
          break;
        case 1:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 1);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelsView.Instance.SP.Children[1]);
          break;
        case 2:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 2);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelsView.Instance.SP.Children[2]);
          break;
        case 3:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 3);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelsView.Instance.SP.Children[3]);
          break;
        case 4:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 4);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelsView.Instance.SP.Children[4]);
          break;
        case 5:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 5);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelsView.Instance.SP.Children[5]);
          break;
        case 6:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 6);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelsView.Instance.SP.Children[6]);
          break;
        case 7:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 7);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelsView.Instance.SP.Children[7]);
          break;
        case 8:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 8);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelsView.Instance.SP.Children[8]);
          break;
        case 9:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 9);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelsView.Instance.SP.Children[9]);
          break;
        case 10:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(SiPMTempCoeff)), this, 0);
          MainViewModel.Instance.NumpadToggleButton(Views.ChannelsView.Instance.CoefTB);
          break;
      }
    }

    public void TextChanged(TextChangedEventArgs e)
    {
      App.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
    }

    public class DropDownButtonContents : Core.ObservableObject
    {
      public string Content
      {
        get => _content;
        set
        {
          _content = value;
          OnPropertyChanged();
        }
      }
      public byte Index { get; set; }
      private static byte _nextIndex = 0;
      private string _content;
      private static ChannelsViewModel _vm;
      public DropDownButtonContents(string content, ChannelsViewModel vm = null)
      {
        if (_vm == null)
        {
          _vm = vm;
        }
        Content = content;
        Index = _nextIndex++;
      }

      public void Click()
      {
        _vm.SelectedSensitivityContent = Content;
        _vm.SelectedSensitivityIndex = Index;
        App.SetSensitivityChannel(Index);
      }

      public static void ResetIndex()
      {
        _nextIndex = 0;
      }
    }
  }
}