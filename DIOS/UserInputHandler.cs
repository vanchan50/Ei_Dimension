﻿using Ei_Dimension.ViewModels;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Controls;
using DIOS.Application;
using Ei_Dimension.Models;
using DIOS.Core.HardwareIntercom;
using DevExpress.XtraPrinting.Native;

namespace Ei_Dimension;

internal static class UserInputHandler
{
  public static (PropertyInfo prop, object VM, int index, TextBox tb) SelectedTextBox
  {
    get { return _selectedTextBox; }
    set {
      if ((value.prop != null) && ((value.prop != _selectedTextBox.prop) || (value.index != _selectedTextBox.index)))
      {
        InputSanityCheck();
      }
      _selectedTextBox = value;
      if (value.prop != null)
      {
        _tempOldString = ((ObservableCollection<string>)_selectedTextBox.prop.GetValue(_selectedTextBox.VM))[_selectedTextBox.index];
        value.tb.Background = (System.Windows.Media.Brush)App.Current.Resources["MenuButtonBackgroundActive"];
      }
      else
        _tempOldString = null;
    }
  }
  private static (PropertyInfo prop, object VM, int index, TextBox tb) _selectedTextBox;
  private static string _tempOldString;
  private static string _tempNewString;
  private static bool _cancelKeyboardInjectionFlag;
  public static bool _disableSanityCheck;

  static UserInputHandler()
  {
#if DEBUG
    _disableSanityCheck = true;
#endif
  }

  public static void InjectToFocusedTextbox(string input, bool keyboardinput = false)
  {
    if (SelectedTextBox.prop != null && !_cancelKeyboardInjectionFlag)
    {
      if (keyboardinput)
      {
        _tempNewString = input;
      }
      else
      {
        _cancelKeyboardInjectionFlag = true;
        _tempNewString = ((ObservableCollection<string>)SelectedTextBox.prop.GetValue(SelectedTextBox.VM))[SelectedTextBox.index];
        if (input == "")
        {
          if (_tempNewString.Length > 0)
            ((ObservableCollection<string>)SelectedTextBox.prop.GetValue(SelectedTextBox.VM))[SelectedTextBox.index] = _tempNewString = _tempNewString.Remove(_tempNewString.Length - 1, 1);
        }
        else
          ((ObservableCollection<string>)SelectedTextBox.prop.GetValue(SelectedTextBox.VM))[SelectedTextBox.index] = _tempNewString += input;
        _cancelKeyboardInjectionFlag = false;
      }
    }
  }

  public static void InputSanityCheck()
  {
    bool failed = false;
    if (SelectedTextBox.prop != null && _tempNewString != null)
    {
      float fRes;
      double dRes;
      int iRes;
      ushort usRes;
      byte bRes;
      string ErrorMessage = null;
      switch (SelectedTextBox.prop.Name)
      {
        case nameof(CalibrationViewModel.Instance.CompensationPercentageContent):
          if (float.TryParse(_tempNewString, out fRes))
          {
            if (fRes is >= 0 and <= 10 || _disableSanityCheck)
            {
              App.DiosApp.Compensation = fRes;
              break;
            }
          }
          failed = true;
          ErrorMessage = "[0-10]";
          break;
        case nameof(CalibrationViewModel.Instance.DNRContents):
          if (SelectedTextBox.index == 0)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if (fRes is >= 1 and <= 300 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.DNRCoefficient, fRes);
                App.DiosApp._beadProcessor.HDnrCoef = fRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1-300]";
          }
          if (SelectedTextBox.index == 1)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if (fRes is >= 1 and <= 40000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.DNRTransition, fRes);
                App.DiosApp._beadProcessor.HdnrTrans = fRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1-40000]";
          }
          break;
        case nameof(DashboardViewModel.Instance.EndRead):
          if (SelectedTextBox.index == 0)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 1) || _disableSanityCheck)
              {
                App.DiosApp.Terminator.MinPerRegion = iRes;
                Settings.Default.MinPerRegion = iRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = "[>0]";
          }
          if (SelectedTextBox.index == 1)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 1) || _disableSanityCheck)
              {
                App.DiosApp.Terminator.TotalBeadsToCapture = iRes;
                Settings.Default.BeadsToCapture = iRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = "[>0]";
          }
          if (SelectedTextBox.index == 2)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 1) || _disableSanityCheck)
              {
                App.DiosApp.Terminator.TerminationTime = iRes;
                Settings.Default.TerminationTimer = iRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = "[>0]";
          }
          break;
        case nameof(DashboardViewModel.Volumes):
          if (SelectedTextBox.index == 0)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 10 and <= 100 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.Volume, VolumeType.Sample, (ushort)iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[10-100]";
          }
          if (SelectedTextBox.index == 1)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 1 and <= 150 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.Volume, VolumeType.Wash, (ushort)iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1-150]";
          }
          if (SelectedTextBox.index == 2)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 1 and <= 500 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.Volume, VolumeType.ProbeWash, (ushort)iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1-500]";
          }
          if (SelectedTextBox.index == 3)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 1 and <= 500 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.Volume, VolumeType.Agitate, (ushort)iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1-500]";
          }
          break;
        case nameof(DashboardViewModel.Repeats):
          if (SelectedTextBox.index == 0)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 10 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.WashRepeatsAmount, (ushort)iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-10]";
          }
          if (SelectedTextBox.index == 1)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 10 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ProbewashRepeatsAmount, (ushort)iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-10]";
          }
          if (SelectedTextBox.index == 2)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 10 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.AgitateRepeatsAmount, (ushort)iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-10]";
          }
          break;
        case nameof(CalibrationViewModel.Instance.EventTriggerContents):
          if (SelectedTextBox.index == 0)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 1 and <= 2000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.Height, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1-2000]";
          }
          if (SelectedTextBox.index == 1)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 20000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.MinSSC, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-20000]";
          }
          if (SelectedTextBox.index == 2)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 30000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.MaxSSC, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-30000]";
          }
          break;
        case nameof(CalibrationViewModel.Instance.ClassificationTargetsContents):
          if (SelectedTextBox.index == 0)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 30000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationTarget, CalibrationTarget.CL0, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-30000]";
          }
          if (SelectedTextBox.index == 1)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 30000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationTarget, CalibrationTarget.CL1, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-30000]";
          }
          if (SelectedTextBox.index == 2)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 30000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationTarget, CalibrationTarget.CL2, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-30000]";
          }
          if (SelectedTextBox.index == 3)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 30000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationTarget, CalibrationTarget.CL3, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-30000]";
          }
          if (SelectedTextBox.index == 4)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 30000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationTarget, CalibrationTarget.RP1, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-30000]";
          }
          break;
        case nameof(CalibrationViewModel.Instance.AttenuationBox):
          if (int.TryParse(_tempNewString, out iRes))
          {
            if (iRes is >= 0 and <= 100 || _disableSanityCheck)
            {
              App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.Attenuation, iRes);
              break;
            }
          }
          failed = true;
          ErrorMessage = "[0-100]";
          break;
        case nameof(SyringeSpeedsViewModel.Instance.SheathSyringeParameters):
          if (SelectedTextBox.index == 0)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 1000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.SyringeSpeedSheath, SyringeSpeed.Normal, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1-1000]";
          }
          if (SelectedTextBox.index == 1)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 1000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.SyringeSpeedSheath, SyringeSpeed.HiSpeed, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1-1000]";
          }
          if (SelectedTextBox.index == 2)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 1000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.SyringeSpeedSheath, SyringeSpeed.HiSensitivity, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1-1000]";
          }
          if (SelectedTextBox.index == 3)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 8000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.SyringeSpeedSheath, SyringeSpeed.Flush, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1-8000]";
          }
          if (SelectedTextBox.index == 4)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 8000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.SyringeSpeedSheath, SyringeSpeed.Pickup, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1-8000]";
          }
          if (SelectedTextBox.index == 5)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 8000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.SyringeSpeedSheath, SyringeSpeed.MaxSpeed, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1-8000]";
          }
          break;
        case nameof(SyringeSpeedsViewModel.Instance.SamplesSyringeParameters):
          if (SelectedTextBox.index == 0)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 1000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.SyringeSpeedSample, SyringeSpeed.Normal, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1-1000]";
          }
          if (SelectedTextBox.index == 1)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 1000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.SyringeSpeedSample, SyringeSpeed.HiSpeed, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1-1000]";
          }
          if (SelectedTextBox.index == 2)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 1000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.SyringeSpeedSample, SyringeSpeed.HiSensitivity, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1-1000]";
          }
          if (SelectedTextBox.index == 3)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 8000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.SyringeSpeedSample, SyringeSpeed.Flush, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1-8000]";
          }
          if (SelectedTextBox.index == 4)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 8000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.SyringeSpeedSample, SyringeSpeed.Pickup, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1-8000]";
          }
          if (SelectedTextBox.index == 5)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 8000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.SyringeSpeedSample, SyringeSpeed.MaxSpeed, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1-8000]";
          }
          break;
        case nameof(SyringeSpeedsViewModel.FluidicPathLengths):
          if (SelectedTextBox.index == 0)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.LoopAVolume, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[>0]";
          }
          if (SelectedTextBox.index == 1)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.LoopBVolume, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[>0]";
          }
          if (SelectedTextBox.index == 2)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.LoopAToPickupNeedle, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[>0]";
          }
          if (SelectedTextBox.index == 3)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.LoopBToPickupNeedle, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[>0]";
          }
          if (SelectedTextBox.index == 4)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.LoopAToFlowcellBase, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[>0]";
          }
          if (SelectedTextBox.index == 5)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.LoopBToFlowcellBase, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[>0]";
          }
          if (SelectedTextBox.index == 6)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.FlowCellNeedleVolume, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[>0]";
          }
          if (SelectedTextBox.index == 7)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.SpacerSlug, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[>0]";
          }
          break;
        case nameof(ChannelOffsetViewModel.SiPMTempCoeff):
          if (float.TryParse(_tempNewString, out fRes))
          {
            if ((fRes >= -10.0000000001 && fRes <= 10.00000000000001) || _disableSanityCheck)
            {
              App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.SiPMTempCoeff, fRes);
              break;
            }
          }
          failed = true;
          ErrorMessage = "[-10.0 - 10.0]";
          break;
        case nameof(ChannelOffsetViewModel.CalibrationMargin):
          if (float.TryParse(_tempNewString, out fRes))
          {
            if ((fRes >= 0 && fRes < 0.1) || _disableSanityCheck)
            {
              App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationMargin, fRes);
              break;
            }
          }
          failed = true;
          ErrorMessage = "[0.0 - 0.1]";
          break;
        case nameof(ChannelOffsetViewModel.ReporterScale):
          if (float.TryParse(_tempNewString, out fRes))
          {
            if ((fRes > 0) || _disableSanityCheck)
            {
              App.DiosApp.ReporterScaling = fRes;
              Settings.Default.ReporterScaling = fRes;
              MainViewModel.Instance.SetScalingMarker(fRes);
              break;
            }
          }
          failed = true;
          ErrorMessage = "[>0]";
          break;
        case nameof(ChannelsViewModel.Instance.Bias30Parameters):
          if (SelectedTextBox.index == 0)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 3500) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 10000))) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ChannelBias30C, Channel.GreenA, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
          }
          if (SelectedTextBox.index == 1)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 3500) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 10000))) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ChannelBias30C, Channel.GreenB, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
          }
          if (SelectedTextBox.index == 2)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 3500) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 10000))) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ChannelBias30C, Channel.GreenC, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
          }
          if (SelectedTextBox.index == 3)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 3500) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 10000))) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ChannelBias30C, Channel.RedA, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
          }
          if (SelectedTextBox.index == 4)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 3500) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 10000))) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ChannelBias30C, Channel.RedB, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
          }
          if (SelectedTextBox.index == 5)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 3500) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 10000))) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ChannelBias30C, Channel.RedC, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
          }
          if (SelectedTextBox.index == 6)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 3500) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 10000))) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ChannelBias30C, Channel.RedD, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
          }
          if (SelectedTextBox.index == 7)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 3500) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 10000))) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ChannelBias30C, Channel.VioletA, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
          }
          if (SelectedTextBox.index == 8)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 3500) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 10000))) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ChannelBias30C, Channel.VioletB, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
          }
          if (SelectedTextBox.index == 9)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 3500) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 10000))) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ChannelBias30C, Channel.ForwardScatter, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
          }
          break;
        case nameof(ChannelOffsetViewModel.Instance.ChannelsOffsetParameters):
          if (SelectedTextBox.index == 0)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535))) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ChannelOffset, Channel.GreenA, iRes);
                ChannelOffsetViewModel.Instance.OverrideSliderChange = true;
                ChannelOffsetViewModel.Instance.SliderValues[0] = (double)iRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          }
          if (SelectedTextBox.index == 1)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535))) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ChannelOffset, Channel.GreenB, iRes);
                ChannelOffsetViewModel.Instance.OverrideSliderChange = true;
                ChannelOffsetViewModel.Instance.SliderValues[1] = (double)iRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          }
          if (SelectedTextBox.index == 2)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535))) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ChannelOffset, Channel.GreenC, iRes);
                ChannelOffsetViewModel.Instance.OverrideSliderChange = true;
                ChannelOffsetViewModel.Instance.SliderValues[2] = (double)iRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          }
          if (SelectedTextBox.index == 3)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535))) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ChannelOffset, Channel.RedA, iRes);
                ChannelOffsetViewModel.Instance.OverrideSliderChange = true;
                ChannelOffsetViewModel.Instance.SliderValues[3] = (double)iRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          }
          if (SelectedTextBox.index == 4)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535))) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ChannelOffset, Channel.RedB, iRes);
                ChannelOffsetViewModel.Instance.OverrideSliderChange = true;
                ChannelOffsetViewModel.Instance.SliderValues[4] = (double)iRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          }
          if (SelectedTextBox.index == 5)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535))) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ChannelOffset, Channel.RedC, iRes);
                ChannelOffsetViewModel.Instance.OverrideSliderChange = true;
                ChannelOffsetViewModel.Instance.SliderValues[5] = (double)iRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          }
          if (SelectedTextBox.index == 6)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535))) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ChannelOffset, Channel.RedD, iRes);
                ChannelOffsetViewModel.Instance.OverrideSliderChange = true;
                ChannelOffsetViewModel.Instance.SliderValues[6] = (double)iRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          }
          if (SelectedTextBox.index == 7)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535))) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ChannelOffset, Channel.VioletA, iRes);
                ChannelOffsetViewModel.Instance.OverrideSliderChange = true;
                ChannelOffsetViewModel.Instance.SliderValues[7] = (double)iRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          }
          if (SelectedTextBox.index == 8)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535))) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ChannelOffset, Channel.VioletB, iRes);
                ChannelOffsetViewModel.Instance.OverrideSliderChange = true;
                ChannelOffsetViewModel.Instance.SliderValues[8] = (double)iRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          }
          if (SelectedTextBox.index == 9)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535))) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ChannelOffset, Channel.GreenC, iRes);
                ChannelOffsetViewModel.Instance.OverrideSliderChange = true;
                ChannelOffsetViewModel.Instance.SliderValues[9] = (double)iRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          }
          break;
        /*
        case "ChannelsBaseline":
          if (SelectedTextBox.index == 0)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535)))
              {
                App.DiosApp.Device.MainCommand("Set Property", code: 0xb8, parameter: (ushort)iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          }
          if (SelectedTextBox.index == 1)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535)))
              {
                App.DiosApp.Device.MainCommand("Set Property", code: 0xb9, parameter: (ushort)iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          }
          if (SelectedTextBox.index == 2)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535)))
              {
                App.DiosApp.Device.MainCommand("Set Property", code: 0xba, parameter: (ushort)iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          }
          if (SelectedTextBox.index == 3)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535)))
              {
                App.DiosApp.Device.MainCommand("Set Property", code: 0xbb, parameter: (ushort)iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          }
          if (SelectedTextBox.index == 4)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535)))
              {
                App.DiosApp.Device.MainCommand("Set Property", code: 0xbc, parameter: (ushort)iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          }
          if (SelectedTextBox.index == 5)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535)))
              {
                App.DiosApp.Device.MainCommand("Set Property", code: 0xbd, parameter: (ushort)iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          }
          if (SelectedTextBox.index == 6)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535)))
              {
                App.DiosApp.Device.MainCommand("Set Property", code: 0xbe, parameter: (ushort)iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          }
          if (SelectedTextBox.index == 7)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535)))
              {
                App.DiosApp.Device.MainCommand("Set Property", code: 0x82, parameter: (ushort)iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          }
          if (SelectedTextBox.index == 8)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535)))
              {
                App.DiosApp.Device.MainCommand("Set Property", code: 0x83, parameter: (ushort)iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          }
          if (SelectedTextBox.index == 9)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535)))
              {
                App.DiosApp.Device.MainCommand("Set Property", code: 0x85, parameter: (ushort)iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          }
          break;
        */
        case nameof(MotorsViewModel.Instance.ParametersX):
          if (SelectedTextBox.index == 0)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 65535 || _disableSanityCheck)
              {
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-65535]";
          }
          if (SelectedTextBox.index == 2)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 1000 and <= 3000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorX, MotorParameterType.Slope, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1000-3000]";
          }
          if (SelectedTextBox.index == 3)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 1000 and <= 5000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorX, MotorParameterType.StartSpeed, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1000-5000]";
          }
          if (SelectedTextBox.index == 4)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 1000 and <= 10000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorX, MotorParameterType.RunSpeed, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1000-10000]";
          }
          if (SelectedTextBox.index == 6)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 200 and <= 2000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorX, MotorParameterType.EncoderSteps, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[200-2000]";
          }
          //if (SelectedTextBox.index == 7)
          //{
          //  if (int.TryParse(_tempNewString, out iRes))
          //  {
          //    if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535))) || _disableSanityCheck)
          //    {
          //      App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorX, MotorParameterType.CurrentLimit, iRes);
          //      break;
          //    }
          //  }
          //  failed = true;
          //  ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          //}
          break;
        case nameof(MotorsViewModel.Instance.ParametersY):
          if (SelectedTextBox.index == 0)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 65535 || _disableSanityCheck)
              {
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-65535]";
          }
          if (SelectedTextBox.index == 2)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 1000 and <= 3000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorY, MotorParameterType.Slope, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1000-3000]";
          }
          if (SelectedTextBox.index == 3)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 1000 and <= 5000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorY, MotorParameterType.StartSpeed, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1000-5000]";
          }
          if (SelectedTextBox.index == 4)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 1000 and <= 10000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorY, MotorParameterType.RunSpeed, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1000-10000]";
          }
          if (SelectedTextBox.index == 6)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 200 and <= 2000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorY, MotorParameterType.EncoderSteps, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[200-2000]";
          }
          //if (SelectedTextBox.index == 7)
          //{
          //  if (int.TryParse(_tempNewString, out iRes))
          //  {
          //    if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535))) || _disableSanityCheck)
          //    {
          //      App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorY, MotorParameterType.CurrentLimit, iRes);
          //      break;
          //    }
          //  }
          //  failed = true;
          //  ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          //}
          break;
        case nameof(MotorsViewModel.Instance.ParametersZ):
          if (SelectedTextBox.index == 0)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 65535 || _disableSanityCheck)
              {
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-65535]";
          }
          if (SelectedTextBox.index == 2)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 1000 and <= 3000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorZ, MotorParameterType.Slope, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1000-3000]";
          }
          if (SelectedTextBox.index == 3)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 1000 and <= 5000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorZ, MotorParameterType.StartSpeed, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1000-5000]";
          }
          if (SelectedTextBox.index == 4)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 1000 and <= 10000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorZ, MotorParameterType.RunSpeed, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[1000-10000]";
          }
          if (SelectedTextBox.index == 6)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 200 and <= 2000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorZ, MotorParameterType.EncoderSteps, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[200-2000]";
          }
          if (SelectedTextBox.index == 7)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535))) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorZ, MotorParameterType.CurrentLimit, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          }
          break;
        case nameof(MotorsViewModel.Instance.StepsParametersX):
          if (SelectedTextBox.index == 0)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if ((fRes >= 0 && fRes <= 20000.0000000001) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorStepsX, MotorStepsX.Plate96Column1, fRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-20000]";
          }
          if (SelectedTextBox.index == 1)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if ((fRes >= 0 && fRes <= 20000.0000000001) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorStepsX, MotorStepsX.Plate96Column12, fRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-20000]";
          }
          if (SelectedTextBox.index == 2)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if ((fRes >= 0 && fRes <= 20000.0000000001) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorStepsX, MotorStepsX.Plate384Column1, fRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-20000]";
          }
          if (SelectedTextBox.index == 3)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if ((fRes >= 0 && fRes <= 20000.0000000001) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorStepsX, MotorStepsX.Plate384Column24, fRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-20000]";
          }
          if (SelectedTextBox.index == 4)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if ((fRes >= 0 && fRes <= 20000.0000000001) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorStepsX, MotorStepsX.Tube, fRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-20000]";
          }
          break;
        case nameof(MotorsViewModel.Instance.StepsParametersY):
          if (SelectedTextBox.index == 0)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if ((fRes >= 0 && fRes <= 20000.0000000001) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorStepsY, MotorStepsY.Plate96RowA, fRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-20000]";
          }
          if (SelectedTextBox.index == 1)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if ((fRes >= 0 && fRes <= 20000.0000000001) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorStepsY, MotorStepsY.Plate96RowH, fRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-20000]";
          }
          if (SelectedTextBox.index == 2)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if ((fRes >= 0 && fRes <= 20000.0000000001) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorStepsY, MotorStepsY.Plate384RowA, fRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-20000]";
          }
          if (SelectedTextBox.index == 3)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if ((fRes >= 0 && fRes <= 20000.0000000001) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorStepsY, MotorStepsY.Plate384RowP, fRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-20000]";
          }
          if (SelectedTextBox.index == 4)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if ((fRes >= 0 && fRes <= 20000.0000000001) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorStepsY, MotorStepsY.Tube, fRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-20000]";
          }
          break;
        case nameof(MotorsViewModel.Instance.StepsParametersZ):
          if (SelectedTextBox.index == 0)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if ((fRes >= 0 && fRes <= 1000.0000000001) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorStepsZ, MotorStepsZ.A1, fRes);
                PlateCustomizationViewModel.Instance.DefaultPlate.A1 = fRes;
                PlateCustomizationViewModel.Instance.UpdateDefault();
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-1000]";
          }
          if (SelectedTextBox.index == 1)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if ((fRes >= 0 && fRes <= 1000.0000000001) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorStepsZ, MotorStepsZ.A12, fRes);
                PlateCustomizationViewModel.Instance.DefaultPlate.A12 = fRes;
                PlateCustomizationViewModel.Instance.UpdateDefault();
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-1000]";
          }
          if (SelectedTextBox.index == 2)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if ((fRes is >= 0 and <= 1000.0000000001f) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorStepsZ, MotorStepsZ.H1, fRes);
                PlateCustomizationViewModel.Instance.DefaultPlate.H1 = fRes;
                PlateCustomizationViewModel.Instance.UpdateDefault();
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-1000]";
          }
          if (SelectedTextBox.index == 3)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if ((fRes is >= 0 and <= 1000.0000000001f) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorStepsZ, MotorStepsZ.H12, fRes);
                PlateCustomizationViewModel.Instance.DefaultPlate.H12 = fRes;
                PlateCustomizationViewModel.Instance.UpdateDefault();
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-1000]";
          }
          if (SelectedTextBox.index == 4)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if ((fRes is >= 0 and <= 1000.0000000001f) || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorStepsZ, MotorStepsZ.Tube, fRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-1000]";
          }
          if (SelectedTextBox.index == 5)
          {
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes is >= 0 and <= 2000 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.WashStationDepth, iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-2000]";
          }
          break;
        case nameof(AlignmentViewModel.Instance.VerificationTolerances):
          if (SelectedTextBox.index == 0)
          {
            if (double.TryParse(_tempNewString, out dRes))
            {
              if ((dRes >= 0) || _disableSanityCheck)
              {
                Settings.Default.ValidatorToleranceReporter = dRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = "Non-Negative";
          }
          if (SelectedTextBox.index == 1)
          {
            if (double.TryParse(_tempNewString, out dRes))
            {
              if ((dRes >= 0) || _disableSanityCheck)
              {
                Settings.Default.ValidatorToleranceClassification = dRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = "Non-Negative";
          }
          if (SelectedTextBox.index == 2)
          {
            if (double.TryParse(_tempNewString, out dRes))
            {
              if ((dRes >= 0) || _disableSanityCheck)
              {
                Settings.Default.ValidatorToleranceMisclassification = dRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = "Non-Negative";
          }
          break;
        case nameof(FileSaveViewModel.Instance.BaseFileName):
          var flag = false;
          foreach (var c in App.InvalidChars)
          {
            if (_tempNewString.Contains(c.ToString()))
            {
              flag = true;
              break;
            }
          }

          if (flag)
          {
            failed = true;
            ErrorMessage = "Invalid Name";
            break;
          }
          App.DiosApp.Publisher.Outfilename = _tempNewString;
          Settings.Default.SaveFileName = _tempNewString;
          break;
        case nameof(ComponentsViewModel.Instance.MaxPressureBox):
          if (float.TryParse(_tempNewString, out fRes))
          {
            if (Settings.Default.PressureUnitsPSI)
            {
              if (fRes is >= 2 and <= 40 || _disableSanityCheck)
              {
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.PressureWarningLevel, fRes);
                break;
              }
            }
            else
            {
              if (fRes is >= (float)(2 * ComponentsViewModel.TOKILOPASCALCOEFFICIENT) and <= (float)(40 * ComponentsViewModel.TOKILOPASCALCOEFFICIENT) || _disableSanityCheck)
              {
                //return to PSI for the FW
                fRes /= (float)ComponentsViewModel.TOKILOPASCALCOEFFICIENT;
                App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.PressureWarningLevel, fRes);
                break;
              }
            }
          }
          failed = true;
          ErrorMessage = Settings.Default.PressureUnitsPSI ? "[2-40]" :
            $"[{(2 * ComponentsViewModel.TOKILOPASCALCOEFFICIENT):F3)}-{(40 * ComponentsViewModel.TOKILOPASCALCOEFFICIENT):F3)}]";
          break;
        case nameof(TemplateSelectViewModel.Instance.TemplateSaveName):
          var flag1 = false;
          foreach (var c in App.InvalidChars)
          {
            if (_tempNewString.Contains(c.ToString()))
            {
              flag1 = true;
              break;
            }
          }

          if (flag1)
          {
            failed = true;
            ErrorMessage = "Invalid Name";
            break;
          }
          TemplateSelectViewModel.Instance.TemplateSaveName[0] = _tempNewString;
          break;
        case nameof(MaintenanceViewModel.Instance.SanitizeSecondsContent):
          if (int.TryParse(_tempNewString, out iRes))
          {
            if (iRes is >= 1 and <= 100 || _disableSanityCheck)
            {
              break;
            }
          }
          failed = true;
          ErrorMessage = "[1-100]";
          break; 
        case nameof(NormalizationViewModel.Instance.NormalizationFactor):
          if (float.TryParse(_tempNewString, out fRes))
          {
            if (fRes is >= 0.9f and <= 0.99f || _disableSanityCheck)
            {
              break;
            }
          }
          failed = true;
          ErrorMessage = "[0.9-0.99]";
          break;
        case nameof(MapRegionData.MFIValue):
          if (int.TryParse(_tempNewString, out iRes))
          {
            if (iRes is >= 0)// && iRes <= 100) || _overrideSanityCheck)
            {
              break;
            }
          }
          failed = true;
          ErrorMessage = ">=0";// "[1-100]";
          break;
        case nameof(ComponentsViewModel.Instance.StatisticsCutoffBox):
          if (double.TryParse(_tempNewString, out dRes))
          {
            if (dRes is >= 5 and <= 45 || _disableSanityCheck)
            {
              Settings.Default.StatisticsTailDiscardPercentage = dRes / 100;
              StatisticsExtension.TailDiscardPercentage = dRes / 100;
              break;
            }
          }
          failed = true;
          ErrorMessage = "[5-45]";
          break;
        case nameof(PlateCustomizationViewModel.Instance.DACCurrentLimit):
          if (int.TryParse(_tempNewString, out iRes))
          {
            if ((iRes >= 0 && ((App.DiosApp.Device.BoardVersion == 0 && iRes <= 4095) || (App.DiosApp.Device.BoardVersion >= 1 && iRes <= 65535))) || _disableSanityCheck)
            {
              break;
            }
          }
          failed = true;
          ErrorMessage = App.DiosApp.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
          break;
        case nameof(SyringeSpeedsViewModel.SampleSyringeSize):
          if (int.TryParse(_tempNewString, out iRes))
          {
            if (iRes is >= 0 and <= 12500 || _disableSanityCheck)
            {
              App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.SampleSyringeSize, iRes);
              break;
            }
          }
          failed = true;
          ErrorMessage = "[0-12500]";
          break;
        case nameof(SyringeSpeedsViewModel.SheathFlushVolume):
          if (int.TryParse(_tempNewString, out iRes))
          {
            if (iRes is >= 0 and <= 12000 || _disableSanityCheck)
            {
              App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.SheathFlushVolume, iRes);
              break;
            }
          }
          failed = true;
          ErrorMessage = "[0-12000]";
          break;
        case nameof(SyringeSpeedsViewModel.EdgeDistance):
          if (int.TryParse(_tempNewString, out iRes))
          {
            if (iRes is >= 0 and <= 150 || _disableSanityCheck)
            {
              App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.DistanceToWellEdge, iRes);
              break;
            }
          }
          failed = true;
          ErrorMessage = "[0-150]";
          break;
        case nameof(SyringeSpeedsViewModel.EdgeHeight):
          if (int.TryParse(_tempNewString, out iRes))
          {
            if (iRes is >= 0 and <= 100 || _disableSanityCheck)
            {
              App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.WellEdgeDeltaHeight, iRes);
              break;
            }
          }
          failed = true;
          ErrorMessage = "[0-100]";
          break;
        case nameof(SyringeSpeedsViewModel.FlushCycles):
          if (int.TryParse(_tempNewString, out iRes))
          {
            if (iRes is >= 0)// && iRes <= 100) || _overrideSanityCheck)
            {
              App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.FlushCycles, iRes);
              break;
            }
          }
          failed = true;
          ErrorMessage = $"[0-{int.MaxValue}]";
          break;
        case nameof(DirectMemoryAccessViewModel.HexCode):
          if (int.TryParse(_tempNewString, NumberStyles.HexNumber, Language.TranslationSource.Instance.CurrentCulture, out iRes))
          {
            if (iRes is >= 0 and <= 255)
            {
              break;
            }
          }
          failed = true;
          ErrorMessage = "[00-FF]";
          break;
        case nameof(DirectMemoryAccessViewModel.IntValue):
          if (int.TryParse(_tempNewString, out iRes))
          {
            if (iRes is >= 0 and <= 65535)
            {
              break;
            }
          }
          failed = true;
          ErrorMessage = "[0-65535]";
          break;
        case nameof(DirectMemoryAccessViewModel.FloatValue):
          if (float.TryParse(_tempNewString, out fRes))
          {
            if (fRes is >= 0 and <= float.MaxValue)
            {
              break;
            }
          }
          failed = true;
          ErrorMessage = $"[0-{float.MaxValue}]";
          break;
        case nameof(MotorsViewModel.TraySteps):
          if (float.TryParse(_tempNewString, out fRes))
          {
            if (fRes is >= 0 and <= float.MaxValue)
            {
              App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.TraySteps, fRes);
              break;
            }
          }
          failed = true;
          ErrorMessage = $"[0-{float.MaxValue}]";
          break;
        case nameof(MotorsViewModel.WashStationXCenterCoordinate):
          if (int.TryParse(_tempNewString, out iRes))
          {
            if (iRes is >= 0 and <= 60000 || _disableSanityCheck)
            {
              App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.WashStationXCenterCoordinate, iRes);
              break;
            }
          }
          failed = true;
          ErrorMessage = "[0-60000]";
          break;
        case nameof(VerificationParametersViewModel.ToleranceItems):
          if (SelectedTextBox.index == 0)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if (fRes is >= 0 and <= 100 || _disableSanityCheck)
              {
                VerificationParametersViewModel.Instance.CurrentRegion.MeanTolerance.GreenSSC = fRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-100]";
          }
          if (SelectedTextBox.index == 1)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if (fRes is >= 0 and <= 100 || _disableSanityCheck)
              {
                VerificationParametersViewModel.Instance.CurrentRegion.MeanTolerance.RedSSC = fRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-100]";
          }
          if (SelectedTextBox.index == 2)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if (fRes is >= 0 and <= 100 || _disableSanityCheck)
              {
                VerificationParametersViewModel.Instance.CurrentRegion.MeanTolerance.Cl1 = fRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-100]";
          }
          if (SelectedTextBox.index == 3)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if (fRes is >= 0 and <= 100 || _disableSanityCheck)
              {
                VerificationParametersViewModel.Instance.CurrentRegion.MeanTolerance.Cl2 = fRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-100]";
          }
          if (SelectedTextBox.index == 4)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if (fRes is >= 0 and <= 100 || _disableSanityCheck)
              {
                VerificationParametersViewModel.Instance.CurrentRegion.MeanTolerance.Reporter = fRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-100]";
          }
          break;
        case nameof(VerificationParametersViewModel.MaxCVItems):
          if (SelectedTextBox.index == 0)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if (fRes is >= 0 and <= 100 || _disableSanityCheck)
              {
                VerificationParametersViewModel.Instance.CurrentRegion.MaxCV.GreenSSC = fRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-100]";
          }
          if (SelectedTextBox.index == 1)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if (fRes is >= 0 and <= 100 || _disableSanityCheck)
              {
                VerificationParametersViewModel.Instance.CurrentRegion.MaxCV.RedSSC = fRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-100]";
          }
          if (SelectedTextBox.index == 2)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if (fRes is >= 0 and <= 100 || _disableSanityCheck)
              {
                VerificationParametersViewModel.Instance.CurrentRegion.MaxCV.Cl1 = fRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-100]";
          }
          if (SelectedTextBox.index == 3)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if (fRes is >= 0 and <= 100 || _disableSanityCheck)
              {
                VerificationParametersViewModel.Instance.CurrentRegion.MaxCV.Cl2 = fRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-100]";
          }
          if (SelectedTextBox.index == 4)
          {
            if (float.TryParse(_tempNewString, out fRes))
            {
              if (fRes is >= 0 and <= 100 || _disableSanityCheck)
              {
                VerificationParametersViewModel.Instance.CurrentRegion.MaxCV.Reporter = fRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = "[0-100]";
          }
          break;
        case nameof(VerificationParametersViewModel.TargetReporter):
          if (float.TryParse(_tempNewString, out fRes))
          {
            if (fRes is >= 0 and <= 1000000 || _disableSanityCheck)
            {
              VerificationParametersViewModel.Instance.CurrentRegion.VerificationTargetReporter = fRes;
              break;
            }
          }
          failed = true;
          ErrorMessage = "[0-1000000]";
          break;
        case nameof(VerificationViewModel.MinCount):

          if (int.TryParse(_tempNewString, out iRes))
          {
            if (iRes is >= 0 and <= 50000 || _disableSanityCheck)
            {
              App.DiosApp.MapController.ActiveMap.minVerBeads = iRes;
              break;
            }
          }
          failed = true;
          ErrorMessage = "[0-50000]";
          break;
      }
      Settings.Default.Save();
      if (failed)
      {
        ((ObservableCollection<string>)SelectedTextBox.prop.GetValue(SelectedTextBox.VM))[SelectedTextBox.index] = _tempOldString;
        //Notification.Show(ErrorMessage);
        SelectedTextBox.tb.Background = System.Windows.Media.Brushes.Red;
        MainViewModel.Instance.HintShow(ErrorMessage, SelectedTextBox.tb);
      }
      else
      {
        MainViewModel.Instance.HintHide();
        if (_tempNewString.TrimStart('0') != "")
        {
          var trimmed =_tempNewString.TrimStart('0');
          ((ObservableCollection<string>)SelectedTextBox.prop.GetValue(SelectedTextBox.VM))[SelectedTextBox.index] = trimmed;
          if (trimmed[0] == '.')
            ((ObservableCollection<string>)SelectedTextBox.prop.GetValue(SelectedTextBox.VM))[SelectedTextBox.index] = $"0{trimmed}";
        }
        else
          ((ObservableCollection<string>)SelectedTextBox.prop.GetValue(SelectedTextBox.VM))[SelectedTextBox.index] = "0";
        SelectedTextBox.tb.Background = (System.Windows.Media.Brush)App.Current.Resources["AppBackground"];
      }
      _tempNewString = null;
      _tempOldString = null;
    }
    else if (SelectedTextBox.prop != null && _tempNewString == null)
    {
      SelectedTextBox.tb.Background = (System.Windows.Media.Brush)App.Current.Resources["AppBackground"];
    }
    SelectedTextBox = (null, null, 0, null);
    App.UnfocusUIElement();
  }
}