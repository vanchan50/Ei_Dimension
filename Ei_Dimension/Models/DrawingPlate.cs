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
    public (int row, int col) CurrentlyReadCell { get; set; }
    public (int row, int col) SelectedCell { get; set; }
    public bool FollowingCurrentCell { get; set; }
    private readonly PlateWell[,] _wells; //actual data
    private int _mode;
    private int _CurrentCorner;
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
      _mode = 96;
      _CurrentCorner = 1;
      _gridSet = false;
      CurrentlyReadCell = (-1, -1);
      SelectedCell = (-1, -1);
      FollowingCurrentCell = true;
    }
    public static DrawingPlate Create()
    {
      return ViewModelSource.Create(() => new DrawingPlate());
    }

    public void ChangeMode(int mode)
    {
      switch (mode)
      {
        case 1:
          break;
        case 96:
          _mode = 96;
          ViewModels.ResultsViewModel.Instance.Buttons384Visible = Visibility.Hidden;
          break;
        case 384:
          _mode = 384;
          ViewModels.ResultsViewModel.Instance.Buttons384Visible = Visibility.Visible;
          break;
        default:
          throw new Exception("Only modes 1, 96 or 384 supported");
      }
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
          _wells[i, j].FilePath = null;
        }
      }
    }

    public void ChangeState(byte row, byte col, WellType type, string FilePath = null)  //TODO: add 384 functionality here, to change appropriate well.
    {
      _wells[row, col].Type = type;
      if (FilePath != null)
        _wells[row, col].FilePath = FilePath;
      if (_mode == 96)
      {
        DrawingWells[row].SetType(col, type);
      }
      else if (_mode == 384)
      {
        var tempCorner = CalculateCorner(row, col);

        if (_CurrentCorner == tempCorner)
        {
          //if currently displayed -> draw
          byte shiftX = 0;
          byte shiftY = 0;
          CorrectionForCorner(_CurrentCorner, ref shiftX, ref shiftY);
          DrawingWells[row - shiftY].SetType(col - shiftX, type);
        }
      }
    }

    public void ChangeCorner(int corner)
    {
      if (_mode != 384 || _CurrentCorner == corner)
        return;
      byte shiftX = 0;
      byte shiftY = 0;
      CorrectionForCorner(corner, ref shiftX, ref shiftY);
      for (var i = 0; i < 8; i++)
      {
        for (var j = 0; j < 12; j++)
        {
          DrawingWells[i].SetType(j, _wells[i + shiftY, j + shiftX].Type);
        }
      }
      _CurrentCorner = corner;

    }

    public void SetWellsForReading(List<MicroCy.Wells> wells)
    {
      foreach(var well in wells)
      {
        ChangeState(well.rowIdx, well.colIdx, WellType.ReadyForReading);
      }
    }

    private static void CorrectionForCorner(int corner, ref byte x, ref byte y)
    {
      switch (corner)
      {
        case 1:
          break;
        case 2:
          x = 12;
          break;
        case 3:
          y = 8;
          break;
        case 4:
          x = 12;
          y = 8;
          break;
        default:
          throw new Exception("Incorrect Argument");
      }
    }

    public static int CalculateCorner(int row, int col)
    {
      var tempCorner = 1;
      if (row < 8)
        tempCorner = col < 12 ? 1 : 2;
      else
        tempCorner = col < 12 ? 3 : 4;
      return tempCorner;
    }

    public (int row, int col) GetSelectedCell() //probably should be a VM function
    {
      if(_drawingGrid != null)
      {
        var SelectedCell = _drawingGrid.CurrentCell;
        if (SelectedCell.IsValid)
        {
          byte shiftX = 0;
          byte shiftY = 0;
          CorrectionForCorner(_CurrentCorner, ref shiftX, ref shiftY);

          return (((WellTableRow)SelectedCell.Item).Index + shiftY, SelectedCell.Column.DisplayIndex + shiftX);
        }
      }
      return (-1,-1);
    }

    public string GetSelectedFilePath()
    {
      return _wells[SelectedCell.row, SelectedCell.col].FilePath;
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
      public string FilePath { get; set; }
      public PlateWell(int row, int col)
      {
        Row = row;
        Column = col;
        Type = WellType.Empty;
      }
    }
  }
}
