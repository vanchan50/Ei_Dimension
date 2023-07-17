﻿using DevExpress.Xpf.Charts;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ei_Dimension.Graphing.HeatMap;

namespace Ei_Dimension.Views
{
  /// <summary>
  /// Interaction logic for ResultsView.xaml
  /// </summary>
  public partial class ResultsView : UserControl, IHeatMapChart
  {
    public static ResultsView Instance;
    public ResultsView()
    {
      InitializeComponent();
      Instance = this;
#if DEBUG
      Console.Error.WriteLine("#6 ResultsView Loaded");
#endif
    }

    public void AddXYPointToHeatMap(SeriesPoint chartPoint, bool LargeXY = false)
    {
      var heatmap = LargeXY ? LargeHeatMap : HeatMap;
      heatmap.Points.Add(chartPoint);
    }

    public void ChangeHeatMapPointColor(int index, SolidColorBrush brush)
    {
      HeatMap.Points[index].Brush = brush;
    }

    public void ClearHeatMaps()
    {
      HeatMap.Points.Clear();
      LargeHeatMap.Points.Clear();
    }

    public void PrintXY(int resoultionDpi = 800)
    {
      ChC.AxisX.Title.Visible = true;
      ChC.AxisY.Title.Visible = true;
      App.Export(XYPlot, resoultionDpi);
      ChC.AxisX.Title.Visible = false;
      ChC.AxisY.Title.Visible = false;
      printXY.Visibility = Visibility.Hidden;
    }

    public void Print3D(int resoultionDpi = 800)
    {
      App.Export(AnalysisPlot, resoultionDpi);
      printAnalysis.Visibility = Visibility.Hidden;
    }

    public void ShowSmallXYPlot()
    {
      HeatMap.Visible = true;
      LargeHeatMap.Visible = false;
      XYPlot.Width = 660;
      XYPlot.Height = 412;
      WldMap.MarkerSize = 5;
      XYPlot.Margin = new Thickness(80, 444, 0, 0);
      printXY.Margin = new Thickness(175, 470, 0, 0);
    }

    public void ShowLargeXYPlot()
    {
      HeatMap.Visible = false;
      LargeHeatMap.Visible = true;
      XYPlot.Width = 1140;
      XYPlot.Height = 856;
      WldMap.MarkerSize = 7;
      XYPlot.Margin = new Thickness(80, 0, 0, 0);
      printXY.Margin = new Thickness(280, 26, 0, 0);
    }

    private void XYPlot_MouseEnter(object sender, MouseEventArgs e)
    {
      printXY.Visibility = Visibility.Visible;
    }

    private void XYPlot_MouseLeave(object sender, MouseEventArgs e)
    {
      printXY.Visibility = Visibility.Hidden;
    }

    private void Plot3D_MouseEnter(object sender, MouseEventArgs e)
    {
      printAnalysis.Visibility = Visibility.Visible;
    }

    private void Plot3D_MouseLeave(object sender, MouseEventArgs e)
    {
      printAnalysis.Visibility = Visibility.Hidden;
    }
  }
}