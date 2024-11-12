using System;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using DIOS.Application;
using DIOS.Core;
using DIOS.Core.HardwareIntercom;

namespace Ei_Dimension.ViewModels;

[POCOViewModel]
public class ChannelOffsetViewModel
{
  public virtual ObservableCollection<string> ChannelsOffsetParameters { get; set; } = new ();
  public virtual ObservableCollection<string> GreenAVoltage { get; set; } = new() { "" };
  public virtual ObservableCollection<string> AverageBg { get; set; } = new ();
  public virtual ObservableCollection<object> SliderValues { get; set; } = new ();
  public bool OverrideSliderChange { get; set; }
  public virtual ObservableCollection<string> SiPMTempCoeff { get; set; } = new() { "" };
  public virtual ObservableCollection<string> CalibrationMargin { get; set; } = new() { "" };
  public virtual ObservableCollection<string> ReporterScale { get; set; }
  //public virtual ObservableCollection<bool> Checkbox { get; set; }
  public virtual string SelectedSensitivityContent { get; set; }
  public virtual ObservableCollection<DropDownButtonContents> SensitivityItems { get; set; }
  public byte SelectedSensitivityIndex { get; set; }
  public double SliderLowLimitGreen => Settings.Default.SanityCheckEnabled ? 21000 : 0;
  public double SliderHighLimitGreen => Settings.Default.SanityCheckEnabled ? 24000 : 65535;
  public double SliderLowLimit => Settings.Default.SanityCheckEnabled ? 40000 : 0;
  public double SliderHighLimit => Settings.Default.SanityCheckEnabled ? 50000 : 65535;
  public static ChannelOffsetViewModel Instance { get; private set; }
  
  protected ChannelOffsetViewModel()
  {
    ReporterScale = new ObservableCollection<string> { Settings.Default.ReporterScaling.ToString($"{0:0.000}") };
    MainViewModel.Instance.SetScalingMarker(Settings.Default.ReporterScaling);
    for (var i = 0; i < 8; i++)
    {
      AverageBg.Add("");
      ChannelsOffsetParameters.Add("");
      SliderValues.Add(new object());
    }
      
    var RM = Language.Resources.ResourceManager;
    var curCulture = Language.TranslationSource.Instance.CurrentCulture;
    SensitivityItems = new ObservableCollection<DropDownButtonContents>
    {
      new(RM.GetString(nameof(Language.Resources.Channels_Sens_GreenB), curCulture), this),
      new(RM.GetString(nameof(Language.Resources.Channels_Sens_GreenC), curCulture), this)
    };
    SelectedSensitivityIndex = Settings.Default.SensitivityChannelB ? (byte)0 : (byte)1;
    SelectedSensitivityContent = SensitivityItems[SelectedSensitivityIndex].Content;

    Instance = this;
  }

  public static ChannelOffsetViewModel Create()
  {
    return ViewModelSource.Create(() => new ChannelOffsetViewModel());
  }

  public void UpdateBiasButtonClick()
  {
    UserInputHandler.InputSanityCheck();
    App.DiosApp.Device.Hardware.SendCommand(DeviceCommandType.RefreshDAC);
    App.InitSTab("channeltab");
  }

  public void SliderValueChanged(int param)
  {
    //if (App.DiosApp.Device.BoardVersion < 1)
    //  return;

    if (OverrideSliderChange)
    {
      OverrideSliderChange = false;
      return;
    }

    var value = (ushort)(double)SliderValues[param];
    Channel channel;
    switch (param)
    {
      case 0:
        channel = Channel.GreenA;
        break;
      case 1:
        channel = Channel.GreenB;
        break;
      case 2:
        channel = Channel.GreenC;
        break;
      case 3:
        channel = Channel.RedA;
        break;
      case 4:
        channel = Channel.RedB;
        break;
      case 5:
        channel = Channel.RedC;
        break;
      case 6:
        channel = Channel.RedD;
        break;
      case 7:
        channel = Channel.GreenD;
        break;
      default:
        throw new NotImplementedException();
    }
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ChannelOffset, channel, value);
    ChannelsOffsetParameters[param] = ((double)SliderValues[param]).ToString();
    App.DiosApp.Device.Hardware.SendCommand(DeviceCommandType.RefreshDAC);
  }

  public void DecodeBackgroundStats(ChannelsAveragesStats Stats)
  {
    AverageBg[0] = Stats.GreenA.ToString($"{0:0.00}");
    AverageBg[1] = Stats.GreenB.ToString($"{0:0.00}");
    AverageBg[2] = Stats.GreenC.ToString($"{0:0.00}");
    AverageBg[3] = Stats.RedA.ToString($"{0:0.00}");
    AverageBg[4] = Stats.RedB.ToString($"{0:0.00}");
    AverageBg[5] = Stats.Cl1.ToString($"{0:0.00}");
    AverageBg[6] = Stats.Cl2.ToString($"{0:0.00}");
    AverageBg[7] = Stats.GreenD.ToString($"{0:0.00}");
  }

  public void FocusedBox(int num)
  {
    var Stackpanel = Views.ChannelOffsetView.Instance.SP.Children;
    //var BaselineStackpanel = Views.ChannelOffsetView.Instance.SP2.Children;
    switch (num)
    {
      case 0:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 0, (TextBox)Stackpanel[0]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Stackpanel[0]);
        break;
      case 1:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 1, (TextBox)Stackpanel[1]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Stackpanel[1]);
        break;
      case 2:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 2, (TextBox)Stackpanel[2]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Stackpanel[2]);
        break;
      case 3:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 3, (TextBox)Stackpanel[3]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Stackpanel[3]);
        break;
      case 4:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 4, (TextBox)Stackpanel[4]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Stackpanel[4]);
        break;
      case 5:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 5, (TextBox)Stackpanel[5]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Stackpanel[5]);
        break;
      case 6:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 6, (TextBox)Stackpanel[6]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Stackpanel[6]);
        break;
      case 7:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 7, (TextBox)Stackpanel[7]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Stackpanel[7]);
        break;
      case 10:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SiPMTempCoeff)), this, 0, Views.ChannelOffsetView.Instance.CoefTB);
        MainViewModel.Instance.NumpadToggleButton(Views.ChannelOffsetView.Instance.CoefTB);
        break;
      case 21:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(CalibrationMargin)), this, 0, Views.ChannelOffsetView.Instance.CalMarginTB);
        MainViewModel.Instance.NumpadToggleButton(Views.ChannelOffsetView.Instance.CalMarginTB);
        break;
      case 22:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ReporterScale)), this, 0, Views.ChannelOffsetView.Instance.RepScalingTB);
        MainViewModel.Instance.NumpadToggleButton(Views.ChannelOffsetView.Instance.RepScalingTB);
        break;
    }
  }

  public void TextChanged(TextChangedEventArgs e)
  {
    UserInputHandler.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
  }

  public void DropPress()
  {
    UserInputHandler.InputSanityCheck();
  }

  private void SetSensitivityChannel(byte num)
  {
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.HiSensitivityChannel, (HiSensitivityChannel)num);
    App.DiosApp._beadProcessor.SensitivityChannel = (HiSensitivityChannel)num;
    Settings.Default.SensitivityChannelB = num == (byte)HiSensitivityChannel.GreenB;
    Settings.Default.Save();
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
    private static ChannelOffsetViewModel _vm;
    public DropDownButtonContents(string content, ChannelOffsetViewModel vm = null)
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
      _vm.SetSensitivityChannel(Index);
    }

    public static void ResetIndex()
    {
      _nextIndex = 0;
    }
  }
}