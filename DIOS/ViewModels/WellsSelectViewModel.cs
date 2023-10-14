using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using Ei_Dimension.Models;
using DIOS.Core;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using DIOS.Application.Domain;
using DIOS.Core.HardwareIntercom;

namespace Ei_Dimension.ViewModels;

[POCOViewModel]
public class WellsSelectViewModel
{
  public virtual Visibility Table96Visible { get; set; } = Visibility.Hidden;
  public virtual Visibility Table384Visible { get; set; } = Visibility.Hidden;
  public virtual ObservableCollection<WellTableRow> Table96Wells { get; set; } = new();
  public virtual ObservableCollection<WellTableRow> Table384Wells { get; set; } = new();
  public int CurrentTableSize { get; private set; } = 0;
  private List<(int row, int col)> _selectedWellIndices = new();
  public static WellsSelectViewModel Instance { get; private set; }
  public virtual bool SquareSelActive { get; set; }

  protected WellsSelectViewModel()
  {
    for (var i = 0; i < 8; i++)
    {
      Table96Wells.Add(new WellTableRow(i, 12)); //ABCD EFGH
    }

    for (var i = 0; i < 16; i++)
    {
      Table384Wells.Add(new WellTableRow(i, 24)); //ABCD EFGH IJKL MNOP
    }

    SquareSelActive = true;
    Instance = this;
  }

  public static WellsSelectViewModel Create()
  {
    return ViewModelSource.Create(() => new WellsSelectViewModel());
  }

  public void SelectIndices(SelectedCellsChangedEventArgs e)
  {
    foreach (var cell in e.AddedCells)
    {
      var columnIndex = cell.Column.DisplayIndex;
      var rowIndex = ((WellTableRow)cell.Item).Index;
      _selectedWellIndices.Add((rowIndex, columnIndex));
    }

    foreach (var cell in e.RemovedCells)
    {
      var columnIndex = cell.Column.DisplayIndex;
      var rowIndex = ((WellTableRow)cell.Item).Index;
      _ = _selectedWellIndices.Remove((rowIndex, columnIndex));
    }
  }

  public void AssignWellTypeButtonClick(int num)
  {
    var plate = CurrentTableSize == 96 ? Table96Wells : Table384Wells;
    foreach (var ind in _selectedWellIndices)
    {
      plate[ind.row].SetType(ind.col, (WellType)num);
    }
  }

  public void ChangeWellTableSize(int num)
  {
    switch (num)
    {
      case 1:
        App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.PlateType, (ushort)PlateSize.Tube);
        Table384Visible = Visibility.Hidden;
        Table96Visible = Visibility.Hidden;
        break;
      case 96:
        Table384Visible = Visibility.Hidden;
        Table96Visible = Visibility.Visible;
        if (CurrentTableSize != num)
        {
          foreach (var row in Table384Wells)
          {
            row.Reset();
          }
        }
        App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.PlateType, (ushort)PlateSize.Plate96);
        break;
      case 384:
        Table384Visible = Visibility.Visible;
        Table96Visible = Visibility.Hidden;
        if (CurrentTableSize != num)
        {
          foreach (var row in Table96Wells)
          {
            row.Reset();
          }
        }
        App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.PlateType, (ushort)PlateSize.Plate384);
        break;
    }
    CurrentTableSize = num;
    PlatePictogramViewModel.Instance.PlatePictogram.ChangeMode(num);
    PlatePictogramViewModel.Instance.CornerButtonClick(1);
    MotorsViewModel.Instance.ChangeAmountOfWells(num);
    Views.WellsSelectView.Instance.ClearAllCells();
    _selectedWellIndices.Clear();
  }

  public void AllSelectClick()
  {
    Views.WellsSelectView.Instance.SelectAllCells(CurrentTableSize);
  }

  public void ToggleSqareSelection()
  {
    SquareSelActive = !SquareSelActive;
    Views.WellsSelectView.SquareSelectionMode = SquareSelActive;
  }

  public void ResetPlateClick()
  {
    var plate = CurrentTableSize == 96 ? Table96Wells : Table384Wells;
    foreach (var row in plate)
    {
      row.Reset();
    }
  }

  public void CustomizePlateClick()
  {
    PlateCustomizationViewModel.Instance.ShowView();
  }

  public List<Well> OutputWells(IReadOnlyCollection<int> regions)
  {
    List<Well> wells;
    if (CurrentTableSize == 1)  //tube
    {
      wells = new List<Well>{ MakeWell(0, 0, regions) };//  a 1 record work order
      return wells;
    }

    wells = GetWellsFromPlate(regions);
    wells = SortWells(wells);
    return wells;
  }

  private Well MakeWell(byte row, byte col, IReadOnlyCollection<int> regions)
  {
    var volRes = uint.Parse(DashboardViewModel.Instance.Volumes[0]);
    var washRes = uint.Parse(DashboardViewModel.Instance.Volumes[1]); 
    var probewashRes = uint.Parse(DashboardViewModel.Instance.Volumes[2]);
    var agitRes = uint.Parse(DashboardViewModel.Instance.Volumes[3]);
    var minPerReg = uint.Parse(DashboardViewModel.Instance.EndRead[0]);
    var totalBeadstoCapture = uint.Parse(DashboardViewModel.Instance.EndRead[1]);
    var terminationTimer = uint.Parse(DashboardViewModel.Instance.EndRead[2]);
    var terminationType = App.DiosApp.Terminator.TerminationType;
    uint washRepeats = uint.Parse(DashboardViewModel.Instance.Repeats[0]);
    uint probeWashRepeats = uint.Parse(DashboardViewModel.Instance.Repeats[1]);
    uint agitateRepeats = uint.Parse(DashboardViewModel.Instance.Repeats[2]);

    return new Well
    (
      row,
      col,
      regions,
      DashboardViewModel.Instance.SelectedSpeedIndex,
      ComponentsViewModel.Instance.SelectedChConfigIndex,
      volRes,
      washRes,
      probewashRes,
      agitRes,
      washRepeats,
      probeWashRepeats,
      agitateRepeats,
      minPerReg,
      totalBeadstoCapture,
      terminationTimer,
      terminationType
    );
  }

  /// <summary>
  /// Sort list of Wells by columns; or keep as is
  /// </summary>
  /// <param name="wells">The list to sort</param>
  /// <returns></returns>
  private List<Well> SortWells(List<Well> wells)
  {
    if (DashboardViewModel.Instance.SelectedOrderIndex == WellReadingOrder.Column)
      wells = wells.OrderBy(x => x.ColIdx).ThenBy(x => x.RowIdx).ToList();
    return wells;
  }

  private List<Well> GetWellsFromPlate(IReadOnlyCollection<int> regions)
  {
    var wells = new List<Well>();
    var plate = CurrentTableSize == 96 ? Table96Wells : Table384Wells;
    for (byte r = 0; r < plate.Count; r++)
    {
      for (byte c = 0; c < plate[r].Types.Count; c++)
      {
        if (plate[r].Types[c] != WellType.Empty)
          wells.Add(MakeWell(r, c, regions));
      }
    }
    return wells;
  }
}