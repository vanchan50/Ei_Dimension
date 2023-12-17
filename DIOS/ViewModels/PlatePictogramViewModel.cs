using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using Ei_Dimension.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace Ei_Dimension.ViewModels;

[POCOViewModel]
public class PlatePictogramViewModel
{
  public virtual Visibility LeftLabel384Visible { get; set; }
  public virtual Visibility RightLabel384Visible { get; set; }
  public virtual Visibility TopLabel384Visible { get; set; }
  public virtual Visibility BottomLabel384Visible { get; set; }
  public virtual Visibility PlatePictogramIsCovered { get; set; }
  public virtual ObservableCollection<bool> CornerButtonsChecked { get; } = new(){ true, false, false, false };
  public virtual Visibility Buttons384Visible { get; set; }
  public virtual DrawingPlate PlatePictogram { get; }

  public static PlatePictogramViewModel Instance { get; private set; }
  protected PlatePictogramViewModel()
  {
    LeftLabel384Visible = Visibility.Visible;
    RightLabel384Visible = Visibility.Hidden;
    TopLabel384Visible = Visibility.Visible;
    BottomLabel384Visible = Visibility.Hidden;
    Buttons384Visible = Visibility.Hidden;
    PlatePictogramIsCovered = Visibility.Hidden;
    PlatePictogram = DrawingPlate.Create();
    Instance = this;
  }

  public static PlatePictogramViewModel Create()
  {
    return ViewModelSource.Create(() => new PlatePictogramViewModel());
  }

  public void CornerButtonClick(int corner)
  {
    var changed = PlatePictogram.ChangeCorner(corner);
    if (!changed)
      return;
    //adjust labels
    switch (corner)
    {
      case 1:
        LeftLabel384Visible = Visibility.Visible;
        RightLabel384Visible = Visibility.Hidden;
        TopLabel384Visible = Visibility.Visible;
        BottomLabel384Visible = Visibility.Hidden;
        break;
      case 2:
        LeftLabel384Visible = Visibility.Hidden;
        RightLabel384Visible = Visibility.Visible;
        TopLabel384Visible = Visibility.Visible;
        BottomLabel384Visible = Visibility.Hidden;
        break;
      case 3:
        LeftLabel384Visible = Visibility.Visible;
        RightLabel384Visible = Visibility.Hidden;
        TopLabel384Visible = Visibility.Hidden;
        BottomLabel384Visible = Visibility.Visible;
        break;
      case 4:
        LeftLabel384Visible = Visibility.Hidden;
        RightLabel384Visible = Visibility.Visible;
        TopLabel384Visible = Visibility.Hidden;
        BottomLabel384Visible = Visibility.Visible;
        break;
    }
    Views.PlatePictogramView.Instance.DrawingPlate.UnselectAllCells();
    CornerButtonsChecked[0] = false;
    CornerButtonsChecked[1] = false;
    CornerButtonsChecked[2] = false;
    CornerButtonsChecked[3] = false;
    CornerButtonsChecked[corner - 1] = true;
  }

  public void ToCurrentButtonClick()
  {
    if (App.DiosApp.Device.IsMeasurementGoing)
      return;
#if DEBUG
    App.Logger.Log(new System.Diagnostics.StackTrace().ToString());
#endif

    ResultsViewModel.Instance.PlotCurrent();

    int tempCorner = 1;
    if (PlatePictogram.CurrentlyReadCell.row < 8)
      tempCorner = PlatePictogram.CurrentlyReadCell.col < 12 ? 1 : 2;
    else
      tempCorner = PlatePictogram.CurrentlyReadCell.col < 12 ? 3 : 4;
    CornerButtonClick(tempCorner);
  }

  public void SelectedCellChanged()
  {
    if (App.DiosApp.Device.IsMeasurementGoing)
      return;
    var temp = PlatePictogram.GetSelectedCell();
    if (temp.row == -1)
      return;
    PlatePictogram.SelectedCell = temp;
    if (temp == PlatePictogram.CurrentlyReadCell)
    {
      ToCurrentButtonClick();
      return;
    }
    ResultsViewModel.Instance.PlotCurrent(false);
    ResultsViewModel.Instance.ClearGraphs(false);
    ResultsViewModel.Instance.FillAllData();
  }
}