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
    

    protected CalibrationViewModel()
    {
      GatingItems = new ObservableCollection<DropDownButtonContents> { new DropDownButtonContents("None", this), new DropDownButtonContents("Green SSC", this),
      new DropDownButtonContents("Red SSC", this), new DropDownButtonContents("Green + Red SSC", this), new DropDownButtonContents("RP bg", this),
      new DropDownButtonContents("Green + RP bg", this), new DropDownButtonContents("Red + RP bg", this), new DropDownButtonContents("Green + Red + RP bg", this),};
      SelectedGatingContent = GatingItems[0].Content;

      EventTriggerContents = new string[3];
      EventTriggerContents[1] = "7000";
      EventTriggerContents[2] = "10000";

      ClassificationTargetsContents = new string[5] { "1", "1", "1", "1", "3500"};

      CalibrationParameter = "Off";
      CompensationPercentageContent = "1";
      DNRContents = new string[2];
      DNRContents[1] = "7200";
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

    public class DropDownButtonContents
    {
      public string Content { get; set; }
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