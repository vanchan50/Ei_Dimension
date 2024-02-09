using System.Collections;
using DevExpress.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using System;

namespace LEDCalibrationTool.ViewModels
{
  public class MainViewModel : ViewModelBase
  {
    public virtual ObservableCollection<string> Values { get; set; } = new();
    public virtual ObservableCollection<object> SliderValues { get; set; } = new();
    public bool OverrideSliderChange { get; set; }
    public ICommand SliderValueChangedCommand { get; init; }
    public ICommand FocusedBoxCommand { get; init; }
    public ICommand TextChangedCommand { get; init; }
    public ICommand LedActivatedCommand { get; init; }

    private BitArray _activeLeds = new(12, false);
    private int? _selectedId;
    

    public MainViewModel()
    {
      for (var i = 0; i < 12; i++)
      {
        Values.Add("0");
        SliderValues.Add(new object());
      }
      LedActivatedCommand = new DelegateCommand<int>(LedActivated);
      SliderValueChangedCommand = new DelegateCommand<int>(SliderValueChanged);
      FocusedBoxCommand = new DelegateCommand<int>(FocusedBox);
      TextChangedCommand = new DelegateCommand<TextChangedEventArgs>(TextChanged);
      App.MainCommand(code: 0xD5);//disable all leds
    }

    public void LedActivated(int num)
    {
      _activeLeds[num] = !_activeLeds[num];
      App.MainCommand(code: 0xD5, parameter: GetIntFromBitArray(_activeLeds));
    }

    public void SliderValueChanged(int param)
    {
      if (OverrideSliderChange)
      {
        OverrideSliderChange = false;
        return;
      }

      _selectedId = null;
      var value = (ushort)(double)SliderValues[param];
      App.MainCommand(code: 0xD5, cmd: (byte)(param + 1), parameter: value );
      Values[param] = ((double)SliderValues[param]).ToString();
    }

    public void FocusedBox(int num)
    {
      _selectedId = num;
    }

    public void TextChanged(TextChangedEventArgs e)
    {
      OverrideSliderChange = true;
      if (int.TryParse(((TextBox) e.Source).Text, out var iRes))
      {
        if (_selectedId is not null &&
            iRes is >= 0 and < 256)
        {
          SliderValues[_selectedId.Value] = (double)iRes;
          App.MainCommand(code: 0xD5, cmd: (byte)(_selectedId + 1), parameter: (ushort)iRes);
          OverrideSliderChange = false;
        }
      }

      if (((TextBox)e.Source).Text is "")
      {
        ((TextBox) e.Source).Text = "0";
      }
    }

    private ushort GetIntFromBitArray(BitArray bitArray)
    {

      if (bitArray.Length > 16)
        throw new ArgumentException("Argument length shall be at most 16 bits.");

      var array = new int[1];
      bitArray.CopyTo(array, 0);
      return (ushort)array[0];

    }
  }
}
