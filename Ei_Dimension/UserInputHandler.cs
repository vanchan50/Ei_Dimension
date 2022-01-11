using Ei_Dimension.ViewModels;
using MicroCy;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Controls;

namespace Ei_Dimension
{
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
        int iRes;
        ushort usRes;
        byte bRes;
        string ErrorMessage = null;
        switch (SelectedTextBox.prop.Name)
        {
          case "CompensationPercentageContent":
            if (float.TryParse(_tempNewString, out fRes))
            {
              if (fRes >= 0 && fRes <= 10)
              {
                MicroCy.InstrumentParameters.Calibration.Compensation = fRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = "Compensation is out of range [0-10]";
            break;
          case "DNRContents":
            if (SelectedTextBox.index == 0)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 1 && fRes <= 300)
                {
                  MicroCy.InstrumentParameters.Calibration.HDnrCoef = fRes;
                  App.Device.MainCommand("Set FProperty", code: 0x20, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "High DNR Coefficient is out of range [1-300]";
            }
            if (SelectedTextBox.index == 1)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 1 && fRes <= 30000)
                {
                  MicroCy.InstrumentParameters.Calibration.HdnrTrans = fRes;
                  break;
                }
              }
              failed = true;
              ErrorMessage = "High DNR Transition is out of range [1-30000]";
            }
            break;
          case "EndRead":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1)
                {
                  MicroCyDevice.MinPerRegion = iRes;
                  Settings.Default.MinPerRegion = iRes;
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Minimum amount of beads should be a positive number";
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1)
                {
                  MicroCyDevice.BeadsToCapture = iRes;
                  Settings.Default.BeadsToCapture = iRes;
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Total amount of bead events should be a positive number";
            }
            break;
          case "Volumes":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 10 && iRes <= 100)
                {
                  App.Device.MainCommand("Set Property", code: 0xaf, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Sample Volume is out of range [10-100]";
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1 && iRes <= 100)
                {
                  App.Device.MainCommand("Set Property", code: 0xac, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Wash Volume is out of range [1-100]";
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1 && iRes <= 500)
                {
                  App.Device.MainCommand("Set Property", code: 0xc4, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Agitate Volume is out of range [1-500]";
            }
            break;
          case "EventTriggerContents":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1 && iRes <= 2000)
                {
                  App.Device.MainCommand("Set Property", code: 0xcd, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Event height is out of range [1-2000]";
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 20000)
                {
                  App.Device.MainCommand("Set Property", code: 0xce, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Min SSC is out of range [0-20000]";
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 30000)
                {
                  App.Device.MainCommand("Set Property", code: 0xcf, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Max SSC is out of range [0-30000]";
            }
            break;
          case "ClassificationTargetsContents":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 30000)
                {
                  App.Device.MainCommand("Set Property", code: 0x8b, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "CL0 Classification Target is out of range [0-30000]";
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 30000)
                {
                  App.Device.MainCommand("Set Property", code: 0x8c, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "CL1 Classification Target is out of range [0-30000]";
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 30000)
                {
                  App.Device.MainCommand("Set Property", code: 0x8d, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "CL2 Classification Target is out of range [0-30000]";
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 30000)
                {
                  App.Device.MainCommand("Set Property", code: 0x8e, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "CL3 Classification Target is out of range [0-30000]";
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 30000)
                {
                  App.Device.MainCommand("Set Property", code: 0x8f, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "RP1 Classification Target is out of range [0-30000]";
            }
            break;
          case "AttenuationBox":
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes >= 0 && iRes <= 100)
              {
                App.Device.MainCommand("Set Property", code: 0xbf, parameter: (ushort)iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "Attenuation is out of range [0-100]";
            break;
          case "SheathSyringeParameters":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 1000)
                {
                  App.Device.MainCommand("Set Property", code: 0x30, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Normal Sheath is out of range [1-1000]";
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 1000)
                {
                  App.Device.MainCommand("Set Property", code: 0x31, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Hi Speed Sheath is out of range [1-1000]";
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 1000)
                {
                  App.Device.MainCommand("Set Property", code: 0x32, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Hi Sens Sheath is out of range [1-1000]";
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 8000)
                {
                  App.Device.MainCommand("Set Property", code: 0x33, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Flush Sheath is out of range [1-8000]";
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 8000)
                {
                  App.Device.MainCommand("Set Property", code: 0x34, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Pickup Sheath is out of range [1-8000]";
            }
            if (SelectedTextBox.index == 5)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 8000)
                {
                  App.Device.MainCommand("Set Property", code: 0x35, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Max Speed is out of range [1-8000]";
            }
            break;
          case "SamplesSyringeParameters":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 1000)
                {
                  App.Device.MainCommand("Set Property", code: 0x38, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Normal Samples is out of range [1-1000]";
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 1000)
                {
                  App.Device.MainCommand("Set Property", code: 0x39, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Hi Speed Samples is out of range [1-1000]";
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 1000)
                {
                  App.Device.MainCommand("Set Property", code: 0x3a, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Hi Sens Samples is out of range [1-1000]";
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 8000)
                {
                  App.Device.MainCommand("Set Property", code: 0x3b, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Flush Samples is out of range [1-8000]";
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 8000)
                {
                  App.Device.MainCommand("Set Property", code: 0x3c, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Pickup Samples is out of range [1-8000]";
            }
            if (SelectedTextBox.index == 5)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 8000)
                {
                  App.Device.MainCommand("Set Property", code: 0x3d, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Max Speed Samples is out of range [1-8000]";
            }
            break;
          case "SiPMTempCoeff":
            if (float.TryParse(_tempNewString, out fRes))
            {
              if (fRes >= -10.0000000001 && fRes <= 10.00000000000001)
              {
                App.Device.MainCommand("Set FProperty", code: 0x02, fparameter: fRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "SiPM Temp erature Coefficient is out of range [-10.0 - 10.0]";
            break;
          case "Bias30Parameters":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((App.Device.BoardVersion == 0 && iRes <= 3500) || (App.Device.BoardVersion >= 1 && iRes <= 10000)))
                {
                  App.Device.MainCommand("Set Property", code: 0x28, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = App.Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
              ErrorMessage = $"Green A (SSC) is out of range {range}";
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((App.Device.BoardVersion == 0 && iRes <= 3500) || (App.Device.BoardVersion >= 1 && iRes <= 10000)))
                {
                  App.Device.MainCommand("Set Property", code: 0x29, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = App.Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
              ErrorMessage = $"Green B (PE) is out of range {range}";
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((App.Device.BoardVersion == 0 && iRes <= 3500) || (App.Device.BoardVersion >= 1 && iRes <= 10000)))
                {
                  App.Device.MainCommand("Set Property", code: 0x2a, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = App.Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
              ErrorMessage = $"Green C PE is out of range {range}";
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((App.Device.BoardVersion == 0 && iRes <= 3500) || (App.Device.BoardVersion >= 1 && iRes <= 10000)))
                {
                  App.Device.MainCommand("Set Property", code: 0x2c, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = App.Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
              ErrorMessage = $"Red A (CL3) is out of range {range}";
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((App.Device.BoardVersion == 0 && iRes <= 3500) || (App.Device.BoardVersion >= 1 && iRes <= 10000)))
                {
                  App.Device.MainCommand("Set Property", code: 0x2d, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = App.Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
              ErrorMessage = $"Red B (SSC) is out of range {range}";
            }
            if (SelectedTextBox.index == 5)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((App.Device.BoardVersion == 0 && iRes <= 3500) || (App.Device.BoardVersion >= 1 && iRes <= 10000)))
                {
                  App.Device.MainCommand("Set Property", code: 0x2e, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = App.Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
              ErrorMessage = $"Red C (CL1) is out of range {range}";
            }
            if (SelectedTextBox.index == 6)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((App.Device.BoardVersion == 0 && iRes <= 3500) || (App.Device.BoardVersion >= 1 && iRes <= 10000)))
                {
                  App.Device.MainCommand("Set Property", code: 0x2f, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = App.Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
              ErrorMessage = $"Red D (CL2) is out of range {range}";
            }
            if (SelectedTextBox.index == 7)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((App.Device.BoardVersion == 0 && iRes <= 3500) || (App.Device.BoardVersion >= 1 && iRes <= 10000)))
                {
                  App.Device.MainCommand("Set Property", code: 0x25, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = App.Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
              ErrorMessage = $"Violet A (SSC) is out of range {range}";
            }
            if (SelectedTextBox.index == 8)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((App.Device.BoardVersion == 0 && iRes <= 3500) || (App.Device.BoardVersion >= 1 && iRes <= 10000)))
                {
                  App.Device.MainCommand("Set Property", code: 0x26, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = App.Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
              ErrorMessage = $"Violet B (CL0) is out of range {range}";
            }
            if (SelectedTextBox.index == 9)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((App.Device.BoardVersion == 0 && iRes <= 3500) || (App.Device.BoardVersion >= 1 && iRes <= 10000)))
                {
                  App.Device.MainCommand("Set Property", code: 0x24, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = App.Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
              ErrorMessage = $"Forward Scatter is out of range {range}";
            }
            break;
          case "ChannelsOffsetParameters":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((App.Device.BoardVersion == 0 && iRes <= 4095) || (App.Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  App.Device.MainCommand("Set Property", code: 0xa0, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = App.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Green A (SSC) is out of range {range}";
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((App.Device.BoardVersion == 0 && iRes <= 4095) || (App.Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  App.Device.MainCommand("Set Property", code: 0xa4, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = App.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Green B (PE) is out of range {range}";
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((App.Device.BoardVersion == 0 && iRes <= 4095) || (App.Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  App.Device.MainCommand("Set Property", code: 0xa5, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = App.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Green C PE is out of range {range}";
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((App.Device.BoardVersion == 0 && iRes <= 4095) || (App.Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  App.Device.MainCommand("Set Property", code: 0xa3, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = App.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Red A (CL3) is out of range {range}";
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((App.Device.BoardVersion == 0 && iRes <= 4095) || (App.Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  App.Device.MainCommand("Set Property", code: 0xa2, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = App.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Red B (SSC) is out of range {range}";
            }
            if (SelectedTextBox.index == 5)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((App.Device.BoardVersion == 0 && iRes <= 4095) || (App.Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  App.Device.MainCommand("Set Property", code: 0xa1, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = App.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Red C (CL1) is out of range {range}";
            }
            if (SelectedTextBox.index == 6)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((App.Device.BoardVersion == 0 && iRes <= 4095) || (App.Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  App.Device.MainCommand("Set Property", code: 0x9f, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = App.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Red D (CL2) is out of range {range}";
            }
            if (SelectedTextBox.index == 7)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((App.Device.BoardVersion == 0 && iRes <= 4095) || (App.Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  App.Device.MainCommand("Set Property", code: 0x9d, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = App.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Violet A (SSC) is out of range {range}";
            }
            if (SelectedTextBox.index == 8)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((App.Device.BoardVersion == 0 && iRes <= 4095) || (App.Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  App.Device.MainCommand("Set Property", code: 0x9c, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = App.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Violet B (CL0) is out of range {range}";
            }
            if (SelectedTextBox.index == 9)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((App.Device.BoardVersion == 0 && iRes <= 4095) || (App.Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  App.Device.MainCommand("Set Property", code: 0x9e, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = App.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Forward Scatter is out of range {range}";
            }
            break;
          case "ParametersX":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 65535)
                {
                  break;
                }
              }
              failed = true;
              ErrorMessage = "X Steps is out of range [0-65535]";
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1000 && iRes <= 3000)
                {
                  App.Device.MainCommand("Set Property", code: 0x53, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "X Slope is out of range [1000-3000]";
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1000 && iRes <= 5000)
                {
                  App.Device.MainCommand("Set Property", code: 0x51, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "X Start Speed is out of range [1000-5000]";
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1000 && iRes <= 10000)
                {
                  App.Device.MainCommand("Set Property", code: 0x52, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "X Run Speed is out of range [1000-10000]";
            }
            if (SelectedTextBox.index == 6)
            {
              if (ushort.TryParse(_tempNewString, out usRes))
              {
                if (usRes >= 200 && usRes <= 2000)
                {
                  App.Device.MainCommand("Set Property", code: 0x50, parameter: (ushort)usRes);
                  Settings.Default.StepsPerRevX = usRes;
                  break;
                }
              }
              failed = true;
              ErrorMessage = "X Encoder Steps is out of range [200-2000]";
            }
            if (SelectedTextBox.index == 7)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((App.Device.BoardVersion == 0 && iRes <= 4095) || (App.Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  App.Device.MainCommand("Set Property", code: 0x90, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = App.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"X Current Limit is out of range {range}";
            }
            break;
          case "ParametersY":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 65535)
                {
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Y Steps is out of range [0-65535]";
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1000 && iRes <= 3000)
                {
                  App.Device.MainCommand("Set Property", code: 0x63, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Y Slope is out of range [1000-3000]";
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1000 && iRes <= 5000)
                {
                  App.Device.MainCommand("Set Property", code: 0x61, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Y Start Speed is out of range [1000-5000]";
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1000 && iRes <= 10000)
                {
                  App.Device.MainCommand("Set Property", code: 0x62, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Y Run Speed is out of range [1000-10000]";
            }
            if (SelectedTextBox.index == 6)
            {
              if (ushort.TryParse(_tempNewString, out usRes))
              {
                if (usRes >= 200 && usRes <= 2000)
                {
                  App.Device.MainCommand("Set Property", code: 0x60, parameter: (ushort)usRes);
                  Settings.Default.StepsPerRevY = usRes;
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Y Encoder Steps is out of range [200-2000]";
            }
            if (SelectedTextBox.index == 7)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((App.Device.BoardVersion == 0 && iRes <= 4095) || (App.Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  App.Device.MainCommand("Set Property", code: 0x91, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = App.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Y Current Limit is out of range {range}";
            }
            break;
          case "ParametersZ":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 65535)
                {
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Z Steps is out of range [0-65535]";
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1000 && iRes <= 3000)
                {
                  App.Device.MainCommand("Set Property", code: 0x43, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Z Slope is out of range [1000-3000]";
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1000 && iRes <= 5000)
                {
                  App.Device.MainCommand("Set Property", code: 0x41, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Z Start Speed is out of range [1000-5000]";
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1000 && iRes <= 10000)
                {
                  App.Device.MainCommand("Set Property", code: 0x42, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Z Run Speed is out of range [1000-10000]";
            }
            if (SelectedTextBox.index == 6)
            {
              if (ushort.TryParse(_tempNewString, out usRes))
              {
                if (usRes >= 200 && usRes <= 2000)
                {
                  App.Device.MainCommand("Set Property", code: 0x40, parameter: (ushort)usRes);
                  Settings.Default.StepsPerRevZ = usRes;
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Z Encoder Steps is out of range [200-2000]";
            }
            if (SelectedTextBox.index == 7)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((App.Device.BoardVersion == 0 && iRes <= 4095) || (App.Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  App.Device.MainCommand("Set Property", code: 0x92, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = App.Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Y Current Limit is out of range {range}";
            }
            break;
          case "StepsParametersX":
            if (SelectedTextBox.index == 0)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 20000.0000000001)
                {
                  App.Device.MainCommand("Set FProperty", code: 0x58, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "X 96W C1 is out of range [0-20000]";
            }
            if (SelectedTextBox.index == 1)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 20000.0000000001)
                {
                  App.Device.MainCommand("Set FProperty", code: 0x5a, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "X 96W C12 is out of range [0-20000]";
            }
            if (SelectedTextBox.index == 2)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 20000.0000000001)
                {
                  App.Device.MainCommand("Set FProperty", code: 0x5c, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "X 384W C1 is out of range [0-20000]";
            }
            if (SelectedTextBox.index == 3)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 20000.0000000001)
                {
                  App.Device.MainCommand("Set FProperty", code: 0x5e, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "X 384W C24 is out of range [0-20000]";
            }
            if (SelectedTextBox.index == 4)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 20000.0000000001)
                {
                  App.Device.MainCommand("Set FProperty", code: 0x56, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "X Tube is out of range [0-20000]";
            }
            break;
          case "StepsParametersY":
            if (SelectedTextBox.index == 0)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 20000.0000000001)
                {
                  App.Device.MainCommand("Set FProperty", code: 0x68, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Y Row A is out of range [0-20000]";
            }
            if (SelectedTextBox.index == 1)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 20000.0000000001)
                {
                  App.Device.MainCommand("Set FProperty", code: 0x6a, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Y Row H is out of range [0-20000]";
            }
            if (SelectedTextBox.index == 2)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 20000.0000000001)
                {
                  App.Device.MainCommand("Set FProperty", code: 0x6c, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Y Row A is out of range [0-20000]";
            }
            if (SelectedTextBox.index == 3)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 20000.0000000001)
                {
                  App.Device.MainCommand("Set FProperty", code: 0x6e, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Y Row P is out of range [0-20000]";
            }
            if (SelectedTextBox.index == 4)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 20000.0000000001)
                {
                  App.Device.MainCommand("Set FProperty", code: 0x66, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Y Tube is out of range [0-20000]";
            }
            break;
          case "StepsParametersZ":
            if (SelectedTextBox.index == 0)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 1000.0000000001)
                {
                  App.Device.MainCommand("Set FProperty", code: 0x48, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Z A1 is out of range [0-1000]";
            }
            if (SelectedTextBox.index == 1)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 1000.0000000001)
                {
                  App.Device.MainCommand("Set FProperty", code: 0x4a, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Z A12 is out of range [0-1000]";
            }
            if (SelectedTextBox.index == 2)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 1000.0000000001)
                {
                  App.Device.MainCommand("Set FProperty", code: 0x4c, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Z H1 is out of range [0-1000]";
            }
            if (SelectedTextBox.index == 3)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 1000.0000000001)
                {
                  App.Device.MainCommand("Set FProperty", code: 0x4e, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Z H12 is out of range [0-1000]";
            }
            if (SelectedTextBox.index == 4)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 1000.0000000001)
                {
                  App.Device.MainCommand("Set FProperty", code: 0x46, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Z Tube is out of range [0-1000]";
            }
            break;
          case "IdexTextBoxInputs":
            if (SelectedTextBox.index == 0)
            {
              if (byte.TryParse(_tempNewString, out bRes))
              {
                if (bRes >= 0 && bRes <= 255)
                {
                  MicroCy.InstrumentParameters.Idex.Pos = bRes;
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Idex Position is out of range [0-255]";
            }
            if (SelectedTextBox.index == 1)
            {
              if (ushort.TryParse(_tempNewString, out usRes))
              {
                if (usRes >= 0 && usRes <= 65535)
                {
                  MicroCy.InstrumentParameters.Idex.Steps = usRes;
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Idex Max Steps is out of range [0-65535]";
            }
            break;
          case "BaseFileName":
            ResultReporter.Outfilename = _tempNewString;
            Settings.Default.SaveFileName = _tempNewString;
            break;
          case "MaxPressureBox":
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes >= 5 && iRes <= 40)
              {
                Settings.Default.MaxPressure = iRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = "Max Pressure is out of range [5-40]";
            break;
          case "TemplateSaveName":
            TemplateSelectViewModel.Instance.TemplateSaveName[0] = _tempNewString;
            break;
          case "SanitizeSecondsContent":
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes >= 1 && iRes <= 100)
              {
                break;
              }
            }
            failed = true;
            ErrorMessage = "UVC Sanitize Seconds is out of range [1-100]";
            break;
        }
        if(VerificationViewModel.Instance.isActivePage)
        {
          if (int.TryParse(_tempNewString, out iRes))
          {
            if (iRes < 0 || iRes > 100000)
            {
              failed = true;
              ErrorMessage = "Reporter value is out of range [0-100000]";
            }
          }
          else
          {
            failed = true;
            ErrorMessage = "Reporter value is out of range [0-100000]";
          }
        }
        Settings.Default.Save();
        if (failed)
        {
          ((ObservableCollection<string>)SelectedTextBox.prop.GetValue(SelectedTextBox.VM))[SelectedTextBox.index] = _tempOldString;
          //ShowNotification(ErrorMessage);
          SelectedTextBox.tb.Background = System.Windows.Media.Brushes.Red;
        }
        else
        {
          if (_tempNewString.TrimStart('0') != "")
            ((ObservableCollection<string>)SelectedTextBox.prop.GetValue(SelectedTextBox.VM))[SelectedTextBox.index] = _tempNewString.TrimStart('0');
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
}
