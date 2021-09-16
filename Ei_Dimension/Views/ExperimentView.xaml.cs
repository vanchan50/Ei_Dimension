using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
  /// Interaction logic for ExperimentView.xaml
  /// </summary>
  public partial class ExperimentView : UserControl
  {
    const int shiftX = 24;
    const int shiftY = 39;
    const int ColWidth = 32;
    const int RowHeight = 32;
    public ExperimentView()
    {
      InitializeComponent();
    }

    private void grd384_TouchMove(object sender, TouchEventArgs e)
    {
      var tp = e.GetTouchPoint(grd384);
      int indexX = (int)Math.Floor((tp.Position.X - shiftX) / ColWidth);
      int indexY = (int)Math.Floor((tp.Position.Y - shiftY) / RowHeight);

      if(indexX >= 0 && indexX < 24 && indexY >= 0 && indexY < 16)
      {
        var c = new DataGridCellInfo(grd384.Items[indexY], grd384.Columns[indexX]);
        if (!grd384.SelectedCells.Contains(c))
          grd384.SelectedCells.Add(c);
      }
    }

    private void grd384_TouchDown(object sender, TouchEventArgs e)
    {
      grd384.SelectedCells.Clear();
    }

    private void grd96_TouchMove(object sender, TouchEventArgs e)
    {
      var tp = e.GetTouchPoint(grd96);
      int indexX = (int)Math.Floor((tp.Position.X - shiftX) / ColWidth);
      int indexY = (int)Math.Floor((tp.Position.Y - shiftY) / RowHeight);

      if (indexX >= 0 && indexX < 12 && indexY >= 0 && indexY < 8)
      {
        var c = new DataGridCellInfo(grd96.Items[indexY], grd96.Columns[indexX]);
        if (!grd96.SelectedCells.Contains(c))
          grd96.SelectedCells.Add(c);
      }
    }

    private void grd96_TouchDown(object sender, TouchEventArgs e)
    {
      grd96.SelectedCells.Clear();
    }
  }
}
