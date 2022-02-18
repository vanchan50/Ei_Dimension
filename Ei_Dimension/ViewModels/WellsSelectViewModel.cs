using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using Ei_Dimension.Models;
using MicroCy;
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
    public virtual Visibility Table96Visible { get; set; }
    public virtual Visibility Table384Visible { get; set; }
    public virtual ObservableCollection<WellTableRow> Table96Wells { get; set; }
    public virtual ObservableCollection<WellTableRow> Table384Wells { get; set; }
    public int CurrentTableSize { get; set; }
    private List<(int row, int col)> _selectedWell96Indices;
    private List<(int row, int col)> _selectedWell384Indices;
    public static WellsSelectViewModel Instance { get; private set; }
    public virtual bool SquareSelActive { get; set; }

    protected WellsSelectViewModel()
    {
      InitTables();

      _selectedWell96Indices = new List<(int, int)>();
      _selectedWell384Indices = new List<(int, int)>();
      CurrentTableSize = 0;
      SquareSelActive = true;
      Instance = this;
    }

    public static WellsSelectViewModel Create()
    {
      return ViewModelSource.Create(() => new WellsSelectViewModel());
    }

    public void SelectIndices(SelectedCellsChangedEventArgs e)
    {
      IList<DataGridCellInfo> selectedCells = e.AddedCells;
      foreach (var cell in selectedCells)
      {
        var columnIndex = cell.Column.DisplayIndex;
        var rowIndex = ((WellTableRow)cell.Item).Index;
        if (CurrentTableSize == 96)
          _selectedWell96Indices.Add((rowIndex, columnIndex));
        else if (CurrentTableSize == 384)
          _selectedWell384Indices.Add((rowIndex, columnIndex));
      }
      IList<DataGridCellInfo> removedCells = e.RemovedCells;
      foreach (var cell in removedCells)
      {
        var columnIndex = cell.Column.DisplayIndex;
        var rowIndex = ((WellTableRow)cell.Item).Index;
        if (CurrentTableSize == 96)
          _ = _selectedWell96Indices.Remove((rowIndex, columnIndex));
        else if (CurrentTableSize == 384)
          _ = _selectedWell384Indices.Remove((rowIndex, columnIndex));
      }
    }

    public void AssignWellTypeButtonClick(int num)
    {
      if (CurrentTableSize == 96)
      {
        foreach (var ind in _selectedWell96Indices)
        {
          Table96Wells[ind.row].SetType(ind.col, (WellType)num);
        }
      }
      else if (CurrentTableSize == 384)
      {
        foreach (var ind in _selectedWell384Indices)
        {
          Table384Wells[ind.row].SetType(ind.col, (WellType)num);
        }
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
              for (var i = 0; i < 24; i++)
              {
                row.SetType(i, WellType.Empty);
              }
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
              for (var i = 0; i < 12; i++)
              {
                row.SetType(i, WellType.Empty);
              }
            }
          }
          App.Device.MainCommand("Set Property", code: 0xab, parameter: 1);
          break;
      }
      CurrentTableSize = num;
      ResultsViewModel.Instance.PlatePictogram.ChangeMode(num);
      ResultsViewModel.Instance.CornerButtonClick(1);
      MotorsViewModel.Instance.ChangeAmountOfWells(num);
      Views.WellsSelectView.Instance.grd96.UnselectAllCells();
      Views.WellsSelectView.Instance.grd384.UnselectAllCells();
      Views.WellsSelectView.Instance.grd96.SelectedCells.Clear();
      Views.WellsSelectView.Instance.grd384.SelectedCells.Clear();
      _selectedWell384Indices.Clear();
      _selectedWell96Indices.Clear();
    }

    private void InitTables()
    {
      Table96Visible = Visibility.Hidden;
      Table384Visible = Visibility.Hidden;
      Table96Wells = new ObservableCollection<WellTableRow>();
      Table384Wells = new ObservableCollection<WellTableRow>();
      Table96Wells.Add(new WellTableRow(0, 12)); //A
      Table96Wells.Add(new WellTableRow(1, 12)); //B
      Table96Wells.Add(new WellTableRow(2, 12)); //C
      Table96Wells.Add(new WellTableRow(3, 12)); //D
      Table96Wells.Add(new WellTableRow(4, 12)); //E
      Table96Wells.Add(new WellTableRow(5, 12)); //F
      Table96Wells.Add(new WellTableRow(6, 12)); //G
      Table96Wells.Add(new WellTableRow(7, 12)); //H

      Table384Wells.Add(new WellTableRow(0, 24)); //A
      Table384Wells.Add(new WellTableRow(1, 24)); //B
      Table384Wells.Add(new WellTableRow(2, 24)); //C
      Table384Wells.Add(new WellTableRow(3, 24)); //D
      Table384Wells.Add(new WellTableRow(4, 24)); //E
      Table384Wells.Add(new WellTableRow(5, 24)); //F
      Table384Wells.Add(new WellTableRow(6, 24)); //G
      Table384Wells.Add(new WellTableRow(7, 24)); //H
      Table384Wells.Add(new WellTableRow(8, 24)); //I
      Table384Wells.Add(new WellTableRow(9, 24)); //J
      Table384Wells.Add(new WellTableRow(10, 24)); //K
      Table384Wells.Add(new WellTableRow(11, 24)); //L
      Table384Wells.Add(new WellTableRow(12, 24)); //M
      Table384Wells.Add(new WellTableRow(13, 24)); //N
      Table384Wells.Add(new WellTableRow(14, 24)); //O
      Table384Wells.Add(new WellTableRow(15, 24)); //P
    }

    public void AllSelectClick()
    {
      if (CurrentTableSize == 96)
        Views.WellsSelectView.Instance.grd96.SelectAllCells();
      else if (CurrentTableSize == 384)
        Views.WellsSelectView.Instance.grd384.SelectAllCells();
    }

    public void ToggleSqareSelection()
    {
      SquareSelActive = !SquareSelActive;
      Views.WellsSelectView.SquareSelectionMode = SquareSelActive;
    }

    public void ResetPlateClick()
    {
      if (CurrentTableSize == 96)
      {
        foreach (var row in Table96Wells)
        {
          for (var i = 0; i < 12; i++)
          {
            row.SetType(i, WellType.Empty);
          }
        }
      }
      else if (CurrentTableSize == 384)
      {
        foreach (var row in Table384Wells)
        {
          for (var i = 0; i < 24; i++)
          {
            row.SetType(i, WellType.Empty);
          }
        }
      }
    }

    public List<Well> OutputWells()
    {
      var wells = new List<Well>();
      if (CurrentTableSize > 1)
      {
        ObservableCollection<WellTableRow> plate = CurrentTableSize == 96 ? Table96Wells : Table384Wells;
        if (DashboardViewModel.Instance.SelectedSystemControlIndex == 0)  //manual control of plate
        {
          for (byte r = 0; r < plate.Count; r++)
          {
            for (byte c = 0; c < plate[r].Types.Count; c++)
            {
              if (plate[r].Types[c] != WellType.Empty)
                wells.Add(MakeWell(r, c));
            }
          }
          if (DashboardViewModel.Instance.SelectedOrderIndex == 0)
          {
            //sort list by col/row
            wells = wells.OrderBy(x => x.colIdx).ThenBy(x => x.rowIdx).ToList();
          }
        }
        else  //Work Order control of plate
        {
          //fill wells from work order
          wells = App.Device.WorkOrder.woWells;
        }
      }
      else if (CurrentTableSize == 1)  //tube
        wells.Add(MakeWell(0, 0));  //  a 1 record work order

      return wells;
    }

    private Well MakeWell(byte row, byte col)
    {
      _ = short.TryParse(DashboardViewModel.Instance.Volumes[0], out var volRes);
      _ = short.TryParse(DashboardViewModel.Instance.Volumes[1], out var washRes);
      _ = short.TryParse(DashboardViewModel.Instance.Volumes[2], out var agitRes);

      return new Well
      {
        rowIdx = row,
        colIdx = col,
        runSpeed = DashboardViewModel.Instance.SelectedSpeedIndex,
        sampVol = volRes,
        washVol = washRes,
        agitateVol = agitRes,
        chanConfig = DashboardViewModel.Instance.SelectedChConfigIndex
      };
    }
  }
}