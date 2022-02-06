using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Controls;
using MicroCy;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ChannelOffsetViewModel
  {
    public virtual ObservableCollection<string> ChannelsOffsetParameters { get; set; }
    public virtual ObservableCollection<string> ChannelsBaseline { get; set; }
    public virtual ObservableCollection<string> AverageBg { get; set; }
    public virtual System.Windows.Visibility OldBoardOffsetsVisible { get; set; }
    public virtual object SliderValue1 { get; set; }
    public virtual object SliderValue2 { get; set; }
    public virtual object SliderValue3 { get; set; }
    public bool OverrideSliderChange { get; set; }
    public virtual ObservableCollection<string> SiPMTempCoeff { get; set; }
    public virtual ObservableCollection<string> CalibrationMargin { get; set; }
    public virtual ObservableCollection<string> ReporterScale { get; set; }
    public virtual ObservableCollection<bool> Checkbox { get; set; }
    public virtual string SelectedSensitivityContent { get; set; }
    public virtual ObservableCollection<DropDownButtonContents> SensitivityItems { get; set; }
    public byte SelectedSensitivityIndex { get; set; }
    public static ChannelOffsetViewModel Instance { get; private set; }
    

    protected ChannelOffsetViewModel()
    {
      ChannelsOffsetParameters = new ObservableCollection<string>();
      ChannelsBaseline = new ObservableCollection<string>();
      AverageBg = new ObservableCollection<string>();
      OldBoardOffsetsVisible = System.Windows.Visibility.Visible;
      SiPMTempCoeff = new ObservableCollection<string> { "" };
      CalibrationMargin = new ObservableCollection<string> { "" };
      ReporterScale = new ObservableCollection<string> { Settings.Default.ReporterScaling.ToString($"{0:0.000}") };
      for (var i = 0; i < 10; i++)
      {
        ChannelsOffsetParameters.Add("");
        ChannelsBaseline.Add("");
        AverageBg.Add("");
      }

      Checkbox = new ObservableCollection<bool> { Settings.Default.SubtractBaseline };
      var param = Settings.Default.SubtractBaseline ? 1 : 0;
      App.Device.MainCommand("Set Property", code: 0x1d, parameter: (ushort)param);
      var RM = Language.Resources.ResourceManager;
      var curCulture = Language.TranslationSource.Instance.CurrentCulture;
      SensitivityItems = new ObservableCollection<DropDownButtonContents>
      {
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Channels_Sens_B), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Channels_Sens_C), curCulture), this)
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
      App.Device.MainCommand("RefreshDac");
      App.Device.InitSTab("channeltab");
    }

    public void SetOffsetClick()
    {
      UserInputHandler.InputSanityCheck();
      App.Device.MainCommand("SetBaseline");
    }

    public void SliderValueChanged(int param)
    {
      if (App.Device.BoardVersion < 1)
        return;

      if (OverrideSliderChange)
      {
        OverrideSliderChange = false;
        return;
      }

      switch (param)
      {
        case 0:
          App.Device.MainCommand("Set Property", code: 0xa0, parameter: (ushort)(double)SliderValue1);
          ChannelsOffsetParameters[0] = ((double)SliderValue1).ToString();
          break;
        case 1:
          App.Device.MainCommand("Set Property", code: 0xa4, parameter: (ushort)(double)SliderValue2);
          ChannelsOffsetParameters[1] = ((double)SliderValue2).ToString();
          break;
        case 2:
          App.Device.MainCommand("Set Property", code: 0xa5, parameter: (ushort)(double)SliderValue3);
          ChannelsOffsetParameters[2] = ((double)SliderValue3).ToString();
          break;
      }
      App.Device.MainCommand("RefreshDac");
    }

    public void FocusedBox(int num)
    {
      var Stackpanel = Views.ChannelOffsetView.Instance.SP.Children;
      var BaselineStackpanel = Views.ChannelOffsetView.Instance.SP2.Children;
      var InnerStackpanel = ((StackPanel)Stackpanel[3]).Children;
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
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 3, (TextBox)InnerStackpanel[0]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)InnerStackpanel[0]);
          break;
        case 4:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 4, (TextBox)InnerStackpanel[1]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)InnerStackpanel[1]);
          break;
        case 5:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 5, (TextBox)InnerStackpanel[2]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)InnerStackpanel[2]);
          break;
        case 6:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 6, (TextBox)InnerStackpanel[3]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)InnerStackpanel[3]);
          break;
        case 7:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 7, (TextBox)InnerStackpanel[4]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)InnerStackpanel[4]);
          break;
        case 8:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 8, (TextBox)InnerStackpanel[5]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)InnerStackpanel[5]);
          break;
        case 9:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 9, (TextBox)InnerStackpanel[6]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)InnerStackpanel[6]);
          break;
        case 10:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SiPMTempCoeff)), this, 0, Views.ChannelOffsetView.Instance.CoefTB);
          MainViewModel.Instance.NumpadToggleButton(Views.ChannelOffsetView.Instance.CoefTB);
          break;
        case 11:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsBaseline)), this, 0, (TextBox)BaselineStackpanel[0]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)BaselineStackpanel[0]);
          break;
        case 12:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsBaseline)), this, 1, (TextBox)BaselineStackpanel[1]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)BaselineStackpanel[1]);
          break;
        case 13:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsBaseline)), this, 2, (TextBox)BaselineStackpanel[2]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)BaselineStackpanel[2]);
          break;
        case 14:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsBaseline)), this, 3, (TextBox)BaselineStackpanel[3]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)BaselineStackpanel[3]);
          break;
        case 15:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsBaseline)), this, 4, (TextBox)BaselineStackpanel[4]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)BaselineStackpanel[4]);
          break;
        case 16:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsBaseline)), this, 5, (TextBox)BaselineStackpanel[5]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)BaselineStackpanel[5]);
          break;
        case 17:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsBaseline)), this, 6, (TextBox)BaselineStackpanel[6]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)BaselineStackpanel[6]);
          break;
        case 18:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsBaseline)), this, 7, (TextBox)BaselineStackpanel[7]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)BaselineStackpanel[7]);
          break;
        case 19:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsBaseline)), this, 8, (TextBox)BaselineStackpanel[8]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)BaselineStackpanel[8]);
          break;
        case 20:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsBaseline)), this, 9, (TextBox)BaselineStackpanel[9]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)BaselineStackpanel[9]);
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

    public void CheckedBox(int num)
    {
      if (num == 0)
      {
        App.Device.MainCommand("Set Property", code: 0x1d, parameter: (ushort)0);
        Checkbox[0] = false;
        Settings.Default.SubtractBaseline = false;
        Settings.Default.Save();
        return;
      }
      Checkbox[0] = true;
      Settings.Default.SubtractBaseline = true;
      Settings.Default.Save();
      App.Device.MainCommand("Set Property", code: 0x1d, parameter: (ushort)1);
    }

    public void TextChanged(TextChangedEventArgs e)
    {
      UserInputHandler.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
    }

    public void DropPress()
    {
      UserInputHandler.InputSanityCheck();
    }

    private static void SetSensitivityChannel(byte num)
    {
      MicroCyDevice.ChannelBIsHiSensitivity = num == 0;
      App.Device.MainCommand("Set Property", code: 0x1e, parameter: (ushort)num);
      Settings.Default.SensitivityChannelB = num == 0;
      Settings.Default.Save();
      ResultsViewModel.Instance.SwapHiSensChannelsStats();
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
        SetSensitivityChannel(Index);
      }

      public static void ResetIndex()
      {
        _nextIndex = 0;
      }
    }
  }
}