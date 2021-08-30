using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System;
using Ei_Dimension.Models;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ExperimentViewModel
  {
    public virtual System.Windows.Visibility Table96Visible { get; set; }
    public virtual System.Windows.Visibility Table384Visible { get; set; }
    public virtual ObservableCollection<WellTableRow> TableWells { get; set; }
    private int _tableSize;
    private List<(int row, int col)> _selectedWellIndices;

    protected ExperimentViewModel()
    {
      _tableSize = 96;
      TableWells = new ObservableCollection<WellTableRow>();
      InitTable(96);

      _selectedWellIndices = new List<(int, int)>();
    }

    public static ExperimentViewModel Create()
    {
      return ViewModelSource.Create(() => new ExperimentViewModel());
    }

    public void SelectIndices(SelectedCellsChangedEventArgs e)
    {
      IList<DataGridCellInfo> selectedCells = e.AddedCells;
      foreach (var cell in selectedCells)
      {
        var columnIndex = cell.Column.DisplayIndex;
        var rowIndex = ((WellTableRow)cell.Item).Index;
        _selectedWellIndices.Add((rowIndex, columnIndex));
      }
      IList<DataGridCellInfo> removedCells = e.RemovedCells;
      foreach (var cell in removedCells)
      {
        var columnIndex = cell.Column.DisplayIndex;
        var rowIndex = ((WellTableRow)cell.Item).Index;
        _ = _selectedWellIndices.Remove((rowIndex, columnIndex));
      }
    }

    public void AssignWellTypeButtonClick(int num)
    {
      foreach (var ind in _selectedWellIndices)
      {
        TableWells[ind.row].SetType(ind.col, (WellType)num);
      }
    }

    public void ChangeWellTableSize(int num)
    {
      if (_tableSize == num)
        return;
      _tableSize = num;
      TableWells.Clear();
      InitTable(num);
    }

    private void InitTable(int size)
    {
      switch (size)
      {
        case 96:
          Table96Visible = System.Windows.Visibility.Visible;
          Table384Visible = System.Windows.Visibility.Hidden;
          TableWells.Add(new WellTableRow(0, 12)); //A
          TableWells.Add(new WellTableRow(1, 12)); //B
          TableWells.Add(new WellTableRow(2, 12)); //C
          TableWells.Add(new WellTableRow(3, 12)); //D
          TableWells.Add(new WellTableRow(4, 12)); //E
          TableWells.Add(new WellTableRow(5, 12)); //F
          TableWells.Add(new WellTableRow(6, 12)); //G
          TableWells.Add(new WellTableRow(7, 12)); //H
          break;
        case 384:
          Table96Visible = System.Windows.Visibility.Hidden;
          Table384Visible = System.Windows.Visibility.Visible;
          TableWells.Add(new WellTableRow(0, 24)); //A
          TableWells.Add(new WellTableRow(1, 24)); //B
          TableWells.Add(new WellTableRow(2, 24)); //C
          TableWells.Add(new WellTableRow(3, 24)); //D
          TableWells.Add(new WellTableRow(4, 24)); //E
          TableWells.Add(new WellTableRow(5, 24)); //F
          TableWells.Add(new WellTableRow(6, 24)); //G
          TableWells.Add(new WellTableRow(7, 24)); //H
          TableWells.Add(new WellTableRow(8, 24)); //I
          TableWells.Add(new WellTableRow(9, 24)); //J
          TableWells.Add(new WellTableRow(10, 24)); //K
          TableWells.Add(new WellTableRow(11, 24)); //L
          TableWells.Add(new WellTableRow(12, 24)); //M
          TableWells.Add(new WellTableRow(13, 24)); //N
          TableWells.Add(new WellTableRow(14, 24)); //O
          TableWells.Add(new WellTableRow(15, 24)); //P
          break;
        default:
          throw new Exception("Wrong table size");
      }
    }
  }
}