using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class CalibrationViewModel
  {
    public virtual string SelectedGatingContent { get; set; }
    public byte SelectedGatingIndex { get; set; }
    public virtual ObservableCollection<DropDownButtonContents> GatingItems { get; set; }
    public virtual byte CalibrationParameter { get; set; }
    public virtual ObservableCollection<bool> CalibrationSelectorState { get; set; }

    public virtual ObservableCollection<string> EventTriggerContents { get; set; }
    public virtual ObservableCollection<string> ClassificationTargetsContents { get; set; }
    public virtual ObservableCollection<string> CompensationPercentageContent { get; set; }
    public virtual ObservableCollection<string> DNRContents { get; set; }
    public virtual ObservableCollection<string> CurrentMapName { get; set; }

    public static CalibrationViewModel Instance { get; private set; }

    protected CalibrationViewModel()
    {
      var RM = Language.Resources.ResourceManager;
      var curCulture = Language.TranslationSource.Instance.CurrentCulture;
      GatingItems = new ObservableCollection<DropDownButtonContents>
      {
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_None), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Green_SSC), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Red_SSC), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Green_Red_SSC), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Rp_bg), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Green_Rp_bg), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Red_Rp_bg), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Green_Red_Rp_bg), curCulture), this)
      };
      SelectedGatingIndex = 0;
      SelectedGatingContent = GatingItems[SelectedGatingIndex].Content;
      EventTriggerContents = new ObservableCollection<string> {"", App.Device.ActiveMap.minmapssc.ToString(), App.Device.ActiveMap.maxmapssc.ToString()};

      ClassificationTargetsContents = new ObservableCollection<string> { "1", "1", "1", "1", "3500"};

      CalibrationParameter = 0;
      CalibrationSelectorState = new ObservableCollection<bool> { true, false, false };

      CompensationPercentageContent = new ObservableCollection<string> { MicroCy.InstrumentParameters.Calibration.Compensation.ToString() };
      DNRContents = new ObservableCollection<string> { "", MicroCy.InstrumentParameters.Calibration.HdnrTrans.ToString() };

      CurrentMapName = new ObservableCollection<string> { App.Device.ActiveMap.mapName };

      Instance = this;
    }

    public static CalibrationViewModel Create()
    {
      return ViewModelSource.Create(() => new CalibrationViewModel());
    }

    public void CalibrationSelector(byte num)
    {
      CalibrationSelectorState[0] = false;
      CalibrationSelectorState[1] = false;
      CalibrationSelectorState[2] = false;
      CalibrationSelectorState[num] = true;
      CalibrationParameter = num;
      //chart2.Series["CLTARGET"].Enabled = false; for num=0 and true for num=1
      App.Device.MainCommand("Set Property", code: 0x1b, parameter: num);
    }

    public void SaveCalibrationToMapClick()
    {
      App.Device.SaveCalVals(App.GetActiveMapIndex());
    }

    public void FocusedBox(int num)
    {
      switch (num)
      {
        case 0:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(CompensationPercentageContent)), this, 0);
          break;
        case 1:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(EventTriggerContents)), this, 0);
          break;
        case 2:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(EventTriggerContents)), this, 1);
          break;
        case 3:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(EventTriggerContents)), this, 2);
          break;
        case 4:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(DNRContents)), this, 0);
          break;
        case 5:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(DNRContents)), this, 1);
          break;
        case 6:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ClassificationTargetsContents)), this, 0);
          break;
        case 7:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ClassificationTargetsContents)), this, 1);
          break;
        case 8:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ClassificationTargetsContents)), this, 2);
          break;
        case 9:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ClassificationTargetsContents)), this, 3);
          break;
        case 10:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ClassificationTargetsContents)), this, 4);
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
      private static CalibrationViewModel _vm;
      public DropDownButtonContents(string content, CalibrationViewModel vm = null)
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
        _vm.SelectedGatingContent = Content;
        App.Device.MainCommand("Set Property", code: 0xca, parameter: (ushort)Index);
        App.Device.ScatterGate = Index;
      }

      public void ForAppUpdater()
      {
        _vm.SelectedGatingContent = Content;
        App.Device.ScatterGate = Index;
      }

      public static void ResetIndex()
      {
        _nextIndex = 0;
      }
    }
  }
}