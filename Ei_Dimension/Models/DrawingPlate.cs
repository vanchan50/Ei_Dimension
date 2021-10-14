using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Ei_Dimension.Models
{
  [POCOViewModel]
  public class DrawingPlate
  {
    public virtual ObservableCollection<WellTableRow> DrawingWells { get; protected set; }  //drawing data
    private readonly PlateWell[,] _wells; //actual data
    private DataGrid _drawingGrid;
    private bool _gridSet;

    protected DrawingPlate()
    {
      DrawingWells = new ObservableCollection<WellTableRow>();
      DrawingWells.Add(new WellTableRow(0, 12)); //A
      DrawingWells.Add(new WellTableRow(1, 12)); //B
      DrawingWells.Add(new WellTableRow(2, 12)); //C
      DrawingWells.Add(new WellTableRow(3, 12)); //D
      DrawingWells.Add(new WellTableRow(4, 12)); //E
      DrawingWells.Add(new WellTableRow(5, 12)); //F
      DrawingWells.Add(new WellTableRow(6, 12)); //G
      DrawingWells.Add(new WellTableRow(7, 12)); //H

      _wells = new PlateWell[16, 24];
      for (var i = 0; i < 16; i++)
      {
        for (var j = 0; j < 24; j++)
        {
          _wells[i, j] = new PlateWell(i, j);
        }
      }
      _gridSet = false;
    }
    public static DrawingPlate Create()
    {
      return ViewModelSource.Create(() => new DrawingPlate());
    }

    public void Clear()
    {
      for (var i = 0; i < 8; i++)
      {
        for (var j = 0; j < 12; j++)
        {
          DrawingWells[i].SetType(j, WellType.Empty);
        }
      }
      for (var i = 0; i < 16; i++)
      {
        for (var j = 0; j < 24; j++)
        {
          _wells[i, j].Type = WellType.Empty;
        }
      }
    }

    public void ChangeState(byte row, byte col, WellType type)  //TODO: add 384 functionality here, to change appropriate well.
    {
      _wells[row, col].Type = type;
      DrawingWells[row].SetType(col, type);
    }

    public void SelectedCellChanged() //probably should be a VM function
    {
      var SelectedCell = _drawingGrid.CurrentCell;
      if (SelectedCell.IsValid)
      {
        ((WellTableRow)SelectedCell.Item).SetType(SelectedCell.Column.DisplayIndex, WellType.Success);
      }
    }
    public void SetGrid(DataGrid grid)
    {
      if (!_gridSet)
      {
        _drawingGrid = grid;
        _gridSet = true;
        return;
      }
      throw new Exception("Grid property was set more than once");
    }

    private class PlateWell
    {
      public int Row { get; }
      public int Column { get; }
      public WellType Type { get; set; }
      public PlateWell(int row, int col)
      {
        Row = row;
        Column = col;
        Type = WellType.Empty;
      }
    }
  }
}
