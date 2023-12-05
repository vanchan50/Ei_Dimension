﻿using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using DIOS.Core.HardwareIntercom;

namespace Ei_Dimension.ViewModels;

[POCOViewModel]
public class SyringeSpeedsViewModel
{
  public virtual ObservableCollection<string> SheathSyringeParameters { get; set; } = new();
  public virtual ObservableCollection<string> SamplesSyringeParameters { get; set; } = new();
  public virtual ObservableCollection<string> FluidicPathLengths { get; set; } = new();
  public virtual ObservableCollection<bool> SingleSyringeMode { get; set; } = new(){ false };
  public virtual ObservableCollection<bool> WellEdgeAgitate { get; set; } = new(){ false };
  public virtual ObservableCollection<string> SampleSyringeSize { get; set; } = new(){""};
  public virtual ObservableCollection<string> SheathFlushVolume { get; set; } = new(){ "" };
  public virtual ObservableCollection<string> EdgeDistance { get; set; } = new(){ "" };
  public virtual ObservableCollection<string> EdgeHeight { get; set; } = new(){ "" };
  public virtual ObservableCollection<string> FlushCycles { get; set; } = new(){ "" };
  public static SyringeSpeedsViewModel Instance { get; private set; }

  protected SyringeSpeedsViewModel()
  {
    for (var i = 0; i < 6; i++)
    {
      SheathSyringeParameters.Add("");
      SamplesSyringeParameters.Add("");
    }

    for (var i = 0; i < 8; i++)
    {
      FluidicPathLengths.Add("");
    }
    Instance = this;
  }

  public static SyringeSpeedsViewModel Create()
  {
    return ViewModelSource.Create(() => new SyringeSpeedsViewModel());
  }

  public void SingleSyringeModeChecked(bool state)
  { 
    UserInputHandler.InputSanityCheck();
    if (state)
      App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.SampleSyringeType, SampleSyringeType.Double);
    else
      App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.SampleSyringeType, SampleSyringeType.Single);
    SingleSyringeMode[0] = state;
  }

  public void WellEdgeAgitateChecked(bool state)
  {
    UserInputHandler.InputSanityCheck();
    if (state)
      App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.IsWellEdgeAgitateActive, 1);
    else
      App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.IsWellEdgeAgitateActive, 0);
    WellEdgeAgitate[0] = state;
  }

  public void FocusedBox(int num)
  {
    var sheathSP = Views.SyringeSpeedsView.Instance.sheathSP.Children;
    var samplesSP = Views.SyringeSpeedsView.Instance.samplesSP.Children;
    var fluidicsSP = Views.SyringeSpeedsView.Instance.fluidicsLengthsSP.Children;
    switch (num)
    {
      case 0:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SheathSyringeParameters)), this, 0, (TextBox)sheathSP[0]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)sheathSP[0]);
        break;
      case 1:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SheathSyringeParameters)), this, 1, (TextBox)sheathSP[1]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)sheathSP[1]);
        break;
      case 2:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SheathSyringeParameters)), this, 2, (TextBox)sheathSP[2]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)sheathSP[2]);
        break;
      case 3:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SheathSyringeParameters)), this, 3, (TextBox)sheathSP[3]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)sheathSP[3]);
        break;
      case 4:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SheathSyringeParameters)), this, 4, (TextBox)sheathSP[4]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)sheathSP[4]);
        break;
      case 5:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SheathSyringeParameters)), this, 5, (TextBox)sheathSP[5]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)sheathSP[5]);
        break;
      case 6:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SamplesSyringeParameters)), this, 0, (TextBox)samplesSP[0]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)samplesSP[0]);
        break;
      case 7:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SamplesSyringeParameters)), this, 1, (TextBox)samplesSP[1]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)samplesSP[1]);
        break;
      case 8:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SamplesSyringeParameters)), this, 2, (TextBox)samplesSP[2]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)samplesSP[2]);
        break;
      case 9:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SamplesSyringeParameters)), this, 3, (TextBox)samplesSP[3]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)samplesSP[3]);
        break;
      case 10:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SamplesSyringeParameters)), this, 4, (TextBox)samplesSP[4]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)samplesSP[4]);
        break;
      case 11:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SamplesSyringeParameters)), this, 5, (TextBox)samplesSP[5]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)samplesSP[5]);
        break;
      case 12:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SampleSyringeSize)), this, 0, Views.SyringeSpeedsView.Instance.SyringeSize);
        MainViewModel.Instance.NumpadToggleButton(Views.SyringeSpeedsView.Instance.SyringeSize);
        break;
      case 13:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SheathFlushVolume)), this, 0, Views.SyringeSpeedsView.Instance.SheathFlush);
        MainViewModel.Instance.NumpadToggleButton(Views.SyringeSpeedsView.Instance.SheathFlush);
        break;
      case 14:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(EdgeDistance)), this, 0, Views.SyringeSpeedsView.Instance.EdgeDistance);
        MainViewModel.Instance.NumpadToggleButton(Views.SyringeSpeedsView.Instance.EdgeDistance);
        break;
      case 15:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(EdgeHeight)), this, 0, Views.SyringeSpeedsView.Instance.DeltaHeight);
        MainViewModel.Instance.NumpadToggleButton(Views.SyringeSpeedsView.Instance.DeltaHeight);
        break;
      case 16:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(FlushCycles)), this, 0, Views.SyringeSpeedsView.Instance.FlushCycles);
        MainViewModel.Instance.NumpadToggleButton(Views.SyringeSpeedsView.Instance.FlushCycles);
        break;
      case 17:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(FluidicPathLengths)), this, 0, (TextBox)fluidicsSP[0]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)fluidicsSP[0]);
        break;
      case 18:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(FluidicPathLengths)), this, 1, (TextBox)fluidicsSP[1]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)fluidicsSP[1]);
        break;
      case 19:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(FluidicPathLengths)), this, 2, (TextBox)fluidicsSP[2]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)fluidicsSP[2]);
        break;
      case 20:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(FluidicPathLengths)), this, 3, (TextBox)fluidicsSP[3]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)fluidicsSP[3]);
        break;
      case 21:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(FluidicPathLengths)), this, 4, (TextBox)fluidicsSP[4]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)fluidicsSP[4]);
        break;
      case 22:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(FluidicPathLengths)), this, 5, (TextBox)fluidicsSP[5]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)fluidicsSP[5]);
        break;
      case 23:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(FluidicPathLengths)), this, 6, (TextBox)fluidicsSP[6]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)fluidicsSP[6]);
        break;
      case 24:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(FluidicPathLengths)), this, 7, (TextBox)fluidicsSP[7]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)fluidicsSP[7]);
        break;
    }
  }

  public void TextChanged(TextChangedEventArgs e)
  {
    UserInputHandler.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
  }
}