using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using Ei_Dimension.Models;
using DIOS.Core;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class WellsSelectViewModel
  {
    public virtual Visibility Table96Visible { get; set; } = Visibility.Hidden;
    public virtual Visibility Table384Visible { get; set; } = Visibility.Hidden;
    public virtual ObservableCollection<WellTableRow> Table96Wells { get; set; } = new ObservableCollection<WellTableRow>();
    public virtual ObservableCollection<WellTableRow> Table384Wells { get; set; } = new ObservableCollection<WellTableRow>();
    public int CurrentTableSize { get; set; } = 0;
    private List<(int row, int col)> _selectedWellIndices = new List<(int, int)>();
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
          App.Device.MainCommand("Set Property", code: 0xab, parameter: 2);
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
          App.Device.MainCommand("Set Property", code: 0xab, parameter: 0);
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
          App.Device.MainCommand("Set Property", code: 0xab, parameter: 1);
          break;
      }
      CurrentTableSize = num;
      ResultsViewModel.Instance.PlatePictogram.ChangeMode(num);
      ResultsViewModel.Instance.CornerButtonClick(1);
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

    public List<Well> OutputWells()
    {
      List<Well> wells;
      if (CurrentTableSize == 1)  //tube
      {
        wells = new List<Well>{ MakeWell(0, 0) };//  a 1 record work order
        return wells;
      }

      if (App.Device.Control == SystemControl.WorkOrder)
      {
        //fill wells from work order
        wells = App.Device.WorkOrder.woWells;
        return wells;
      }

      wells = GetWellsFromPlate();
      wells = SortWells(wells);
      return wells;
    }

    private Well MakeWell(byte row, byte col)
    {
      _ = short.TryParse(DashboardViewModel.Instance.Volumes[0], out var volRes);
      _ = short.TryParse(DashboardViewModel.Instance.Volumes[1], out var washRes);
      _ = short.TryParse(DashboardViewModel.Instance.Volumes[2], out var agitRes);

      return new Well
      {
        RowIdx = row,
        ColIdx = col,
        RunSpeed = DashboardViewModel.Instance.SelectedSpeedIndex,
        SampVol = volRes,
        WashVol = washRes,
        AgitateVol = agitRes,
        ChanConfig = DashboardViewModel.Instance.SelectedChConfigIndex,
        MinPerRegion = int.Parse(DashboardViewModel.Instance.EndRead[0]),
        BeadsToCapture = int.Parse(DashboardViewModel.Instance.EndRead[1]),
        TermType = (Termination)DashboardViewModel.Instance.SelectedEndReadIndex
      };
    }
    /// <summary>
    /// Sort list of Wells by columns; or keep as is
    /// </summary>
    /// <param name="wells">The list to sort</param>
    /// <returns></returns>
    private List<Well> SortWells(List<Well> wells)
    {
      if (DashboardViewModel.Instance.SelectedOrderIndex == 0)
        wells = wells.OrderBy(x => x.ColIdx).ThenBy(x => x.RowIdx).ToList();
      return wells;
    }

    private List<Well> GetWellsFromPlate()
    {
      var wells = new List<Well>();
      ObservableCollection<WellTableRow> plate = CurrentTableSize == 96 ? Table96Wells : Table384Wells;
      for (byte r = 0; r < plate.Count; r++)
      {
        for (byte c = 0; c < plate[r].Types.Count; c++)
        {
          if (plate[r].Types[c] != WellType.Empty)
            wells.Add(MakeWell(r, c));
        }
      }
      return wells;
    }
  }
}