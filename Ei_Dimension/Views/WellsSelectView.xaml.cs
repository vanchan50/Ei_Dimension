using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ei_Dimension.Views
{
  /// <summary>
  /// Interaction logic for WellsSelectView.xaml
  /// </summary>
  public partial class WellsSelectView : UserControl
  {
    public static WellsSelectView Instance { get; private set; }
    public static bool SquareSelectionMode { get; set; }
    private readonly int shiftX384 = 0;
    private readonly int shiftY384 = 0;
    private readonly int ColWidth384;
    private readonly int RowHeight384;
    private readonly int shiftX96 = 0;
    private readonly int shiftY96 = 0;
    private readonly int ColWidth96;
    private readonly int RowHeight96;
    private (int x, int y) _basis;
    private (int x, int y) _previousPoint;

    public WellsSelectView()
    {
      InitializeComponent();
      ColWidth96 = (int)(double)App.Current.Resources["Table96Width"];
      RowHeight96 = (int)(double)App.Current.Resources["Table96Width"];
      ColWidth384 = (int)(double)App.Current.Resources["Table384Width"];
      RowHeight384 = (int)(double)App.Current.Resources["Table384Width"];
      Instance = this;
      SquareSelectionMode = true;
    }

    private void grd384_TouchMove(object sender, TouchEventArgs e)
    {
      var tp = e.GetTouchPoint(grd384);
      int indexX = (int)Math.Floor((tp.Position.X - shiftX384) / ColWidth384);
      int indexY = (int)Math.Floor((tp.Position.Y - shiftY384) / RowHeight384);

      if (indexX >= 0 && indexX < 24 && indexY >= 0 && indexY < 16)
      {
        TouchSelection(indexX, indexY, grd384);
      }
    }

    private void grd384_TouchDown(object sender, TouchEventArgs e)
    {
      grd384.SelectedCells.Clear();
    }

    private void grd96_TouchMove(object sender, TouchEventArgs e)
    {
      var tp = e.GetTouchPoint(grd96);
      int indexX = (int)Math.Floor((tp.Position.X - shiftX96) / ColWidth96);
      int indexY = (int)Math.Floor((tp.Position.Y - shiftY96) / RowHeight96);

      if (indexX >= 0 && indexX < 12 && indexY >= 0 && indexY < 8)
      {
        TouchSelection(indexX, indexY, grd96);
      }
    }

    private void grd96_TouchDown(object sender, TouchEventArgs e)
    {
      grd96.SelectedCells.Clear();
    }

    private void TouchSelection(int x, int y, DataGrid grid)
    {

      var c = new DataGridCellInfo(grid.Items[y], grid.Columns[x]);
      if (!SquareSelectionMode)
      {
        if (!grid.SelectedCells.Contains(c))
          grid.SelectedCells.Add(c);
      }
      else
      {
        if (grid.SelectedCells.Count == 0)
        {
          grid.SelectedCells.Add(c);
          _basis = (x, y);
          _previousPoint = (x, y);
        }
        else
        {
          if (_previousPoint.x == x && _previousPoint.y == y)
            return;
          _previousPoint = (x, y);
          grid.SelectedCells.Clear();

          int xStart;
          int xEnd;
          int yStart;
          int yEnd;
          if (x >= _basis.x)
          {
            xStart = _basis.x;
            xEnd = x;
          }
          else
          {
            xStart = x;
            xEnd = _basis.x;
          }
          if (y >= _basis.y)
          {
            yStart = _basis.y;
            yEnd = y;
          }
          else
          {
            yStart = y;
            yEnd = _basis.y;
          }

          for (var i = xStart; i <= xEnd; i++)
          {
            for (var j = yStart; j <= yEnd; j++)
            {
              grid.SelectedCells.Add(new DataGridCellInfo(grid.Items[j], grid.Columns[i]));
            }
          }
        }
      }
    }
  }
}