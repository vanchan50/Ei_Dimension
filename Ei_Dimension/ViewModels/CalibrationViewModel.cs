using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class CalibrationViewModel
  {
    public virtual string SelectedGatingContent { get; set; }
    public virtual ObservableCollection<DropDownButtonContents> GatingItems { get; set; }
    public virtual string CalibrationParameter { get; set; }

    public virtual string[] EventTriggerContents { get; set; }
    public virtual string[] ClassificationTargetsContents { get; set; }
    public virtual string CompensationPercentageContent { get; set; }
    public virtual string[] DNRContents { get; set; }

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
      SelectedGatingContent = GatingItems[0].Content;

      EventTriggerContents = new string[3];
      EventTriggerContents[1] = "7000";
      EventTriggerContents[2] = "10000";

      ClassificationTargetsContents = new string[5] { "1", "1", "1", "1", "3500"};

      CalibrationParameter = "Off";
      CompensationPercentageContent = "1";
      DNRContents = new string[2];
      DNRContents[1] = "7200";
      Instance = this;
    }

    public static CalibrationViewModel Create()
    {
      return ViewModelSource.Create(() => new CalibrationViewModel());
    }

    public void CalibrationSelector(string s)
    {
      CalibrationParameter = s;
    }

    public void SaveCalibrationToMapClick()
    {

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
      private string _content;
      private static CalibrationViewModel _vm;
      public DropDownButtonContents(string content, CalibrationViewModel vm = null)
      {
        if (_vm == null)
        {
          _vm = vm;
        }
        Content = content;
      }

      public void Click()
      {
        _vm.SelectedGatingContent = Content;
      }
    }
  }
}