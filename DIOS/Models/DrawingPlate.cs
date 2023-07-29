using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DIOS.Application.Domain;
using DIOS.Core;

namespace Ei_Dimension.Models;

[POCOViewModel]
public class DrawingPlate
{
  public virtual ObservableCollection<WellTableRow> DrawingWells { get; protected set; }  //drawing data
  public (int row, int col) CurrentlyReadCell { get; set; }
  public (int row, int col) SelectedCell { get; set; }
  private readonly PlateWell[,] _wells; //actual data
  private Warning[,] _warnings;
  private int _mode;
  private int _CurrentCorner;
  private DataGrid _drawingGrid;
  private bool _gridSet;
  private bool _multitubeOverrideReset;

  protected DrawingPlate()
  {
    DrawingWells = new ObservableCollection<WellTableRow>
    {
      new WellTableRow(0, 12), //A
      new WellTableRow(1, 12), //B
      new WellTableRow(2, 12), //C
      new WellTableRow(3, 12), //D
      new WellTableRow(4, 12), //E
      new WellTableRow(5, 12), //F
      new WellTableRow(6, 12), //G
      new WellTableRow(7, 12)  //H
    };

    _wells = new PlateWell[16, 24];
    for (var i = 0; i < 16; i++)
    {
      for (var j = 0; j < 24; j++)
      {
        _wells[i, j] = new PlateWell();
      }
    }
    _mode = 96;
    _CurrentCorner = 1;
    _gridSet = false;
    CurrentlyReadCell = (-1, -1);
    SelectedCell = (-1, -1);
    _multitubeOverrideReset = false;
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
        _mode = 1;
        ViewModels.PlatePictogramViewModel.Instance.Buttons384Visible = Visibility.Hidden;
        break;
      case 96:
        _mode = 96;
        ViewModels.PlatePictogramViewModel.Instance.Buttons384Visible = Visibility.Hidden;
        break;
      case 384:
        _mode = 384;
        ViewModels.PlatePictogramViewModel.Instance.Buttons384Visible = Visibility.Visible;
        break;
      default:
        throw new Exception("Only modes 1, 96 or 384 supported");
    }
    _multitubeOverrideReset = false;
  }

  public void Clear()
  {
    if (_multitubeOverrideReset)
    {
      return;
    }

    for (var i = 0; i < 8; i++)
    {
      for (var j = 0; j < 12; j++)
      {
        DrawingWells[i].SetType(j, WellType.Empty);
        _warnings[i, j].SetWarning(WellWarningState.OK);
      }
    }
    for (var i = 0; i < 16; i++)
    {
      for (var j = 0; j < 24; j++)
      {
        _wells[i, j].Type = WellType.Empty;
        _wells[i, j].FilePath = null;
        _wells[i, j].WarningState = WellWarningState.OK;
      }
    }

    if (_mode == 1)
      _multitubeOverrideReset = true;
  }

  public void ChangeState(int row, int col, WellType? type = null, WellWarningState? warning = null, string FilePath = null)
  {
    if(type != null)
      _wells[row, col].Type = (WellType)type;
    if(warning != null)
      _wells[row, col].WarningState = (WellWarningState)warning;
    if (FilePath != null)
      _wells[row, col].FilePath = FilePath;
    if (_mode == 96 || _mode == 1)
    {
      DrawingWells[row].SetType(col, _wells[row, col].Type);
      _warnings[row, col].SetWarning(_wells[row, col].WarningState);
    }
    else if (_mode == 384)
    {
      if (_CurrentCorner == CalculateCorner(row, col))
      {
        //if currently displayed -> draw
        byte shiftX = 0;
        byte shiftY = 0;
        CorrectionForCorner(_CurrentCorner, ref shiftX, ref shiftY);
        DrawingWells[row - shiftY].SetType(col - shiftX, _wells[row, col].Type);
        _warnings[row - shiftY, col - shiftX].SetWarning(_wells[row, col].WarningState);
      }
    }
  }

  public bool ChangeCorner(int corner)
  {
    if (_mode != 384 || _CurrentCorner == corner)
      return false;
    byte shiftX = 0;
    byte shiftY = 0;
    CorrectionForCorner(corner, ref shiftX, ref shiftY);
    for (var i = 0; i < 8; i++)
    {
      for (var j = 0; j < 12; j++)
      {
        DrawingWells[i].SetType(j, _wells[i + shiftY, j + shiftX].Type);
        _warnings[i, j].SetWarning(_wells[i + shiftY, j + shiftX].WarningState);
      }
    }
    _CurrentCorner = corner;
    return true;
  }

  public void SetWellsForReading(IReadOnlyCollection<Well> wells)
  {
    //Multitube case Override
    if (ViewModels.WellsSelectViewModel.Instance.CurrentTableSize == 1)
      return;
    foreach(var well in wells)
    {
      ChangeState(well.RowIdx, well.ColIdx, WellType.ReadyForReading);
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
    int tempCorner;
    if (row < 8)
      tempCorner = col < 12 ? 1 : 2;
    else
      tempCorner = col < 12 ? 3 : 4;
    return tempCorner;
  }

  public (int row, int col) GetSelectedCell() //probably should be a VM function
  {
    if (_drawingGrid != null)
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

  public void SetWarningGrid(Grid grid)
  {
    Warning.SetGrid(grid);
    _warnings = new Warning[8, 12];
    for (var i = 0; i < 8; i++)
    {
      for (var j = 0; j < 12; j++)
      {
        _warnings[i, j] = new Warning(i, j);
      }
    }
  }
  /// <summary>
  /// Serialize WellType and Warnings.
  /// </summary>
  /// <returns>Returns a JSON string of a plate pictogram. Returns null if a Tube is selcted.</returns>
  public string GetSerializedPlate()
  {
    (int, int)[,] arr = null;
    if(_mode == 96)
    {
      arr = new (int, int)[8,12];
      for(var i = 0; i < 8; i++)
      {
        for(var j = 0; j < 12; j++)
        {
          arr[i, j] = ((int)_wells[i, j].Type, (int)_wells[i, j].WarningState);
        }
      }
    }
    if(_mode == 384)
    {
      arr = new (int, int)[16, 24];
      for (var i = 0; i < 16; i++)
      {
        for (var j = 0; j < 24; j++)
        {
          arr[i, j] = ((int)_wells[i, j].Type, (int)_wells[i, j].WarningState);
        }
      }
    }
    if (_mode == 1)
    {
      arr = new (int, int)[1,1];
      arr[0, 0] = ((int)_wells[0, 0].Type, (int)_wells[0, 0].WarningState);
    }
    var res =  Newtonsoft.Json.JsonConvert.SerializeObject(arr);
    return res;
  }

  private class PlateWell
  {
    public WellType Type { get; set; }
    public string FilePath { get; set; }
    public WellWarningState WarningState { get; set; }
    public PlateWell()
    {
      Type = WellType.Empty;
      WarningState = WellWarningState.OK;
    }
  }

  private class Warning
  {
    private Border _rect;
    private static Grid _warningGrid;
    private static bool _warningGridSet = false;
    public Warning(int row, int col)
    {
      _rect = new Border();
      _rect.Width = 50;
      _rect.Height = 50;
      _rect.Margin = new Thickness(col * 50, row * 50, 0, 0);
      _rect.HorizontalAlignment = HorizontalAlignment.Left;
      _rect.VerticalAlignment = VerticalAlignment.Top;
      _rect.Background = Brushes.Transparent;
      _rect.BorderThickness = new Thickness(3);
      _rect.CornerRadius = new CornerRadius(22);
      _warningGrid.Children.Add(_rect);
    }

    public void SetWarning(WellWarningState warning)
    {
      _ = App.Current.Dispatcher.BeginInvoke(() =>
      {
        switch (warning)
        {
          case WellWarningState.OK:
            _rect.Background = Brushes.Transparent;
            break;
          case WellWarningState.YellowWarning:
            _rect.Background = Brushes.Orange;
            break;
        }
      });
    }

    public static void SetGrid(Grid grid)
    {
      if (!_warningGridSet)
      {
        _warningGrid = grid;
        _warningGridSet = true;
        return;
      }
      throw new Exception("WarningGrid property was set more than once");
    }
  }
}