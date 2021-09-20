using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Map;
using System;
using Ei_Dimension.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class DashboardViewModel
  {
    public virtual Visibility ValidateBCodeButtonVisible { get; set; }
    public virtual ObservableCollection<string> EndRead { get; set; }
    public virtual ObservableCollection<bool> SystemControlSelectorState { get; set; }
    public virtual bool ValidateBCodeButtonEnabled { get; set; }
    public virtual byte OrderSelectorState { get; set; }
    public virtual ObservableCollection<bool> OrderSelectorStateBool { get; set; }
    public virtual ObservableCollection<bool> EndReadSelectorState { get; set; }
    public virtual ObservableCollection<string> Volumes { get; set; }
    public virtual string SelectedSpeedContent { get; set; }
    public virtual ObservableCollection<DropDownButtonContents> SpeedItems { get; set; }
    public virtual string SelectedClassiMapContent { get; set; }
    public virtual ObservableCollection<DropDownButtonContents> ClassiMapItems { get; set; }
    public virtual string SelectedChConfigContent { get; set; }
    public virtual ObservableCollection<DropDownButtonContents> ChConfigItems { get; set; }

    public virtual ObservableCollection<string> EventCountField { get; set; }
    public virtual bool StartButtonEnabled { get; set; }



    public virtual ObservableCollection<string> PressureMon { get; set; }
    public virtual bool PressureMonToggleButtonState { get; set; }
    public double MaxPressure { get; set; }
    public double MinPressure { get; set; }
    public virtual ObservableCollection<string> ActiveList { get; set; }

    public static DashboardViewModel Instance { get; private set; }

    private byte _systemControlSelectorIndex;
    private byte _selectedSpeedIndex;
    private byte _selectedChConfigIndex;

    protected DashboardViewModel()
    {
      EndRead = new ObservableCollection<string> { "100", "500" };
      SystemControlSelectorState = new ObservableCollection<bool> { false, false, false };
      SystemControlSelector(Settings.Default.SystemControl);
      if (_systemControlSelectorIndex == 2)
        ValidateBCodeButtonVisible = Visibility.Visible;
      else
        ValidateBCodeButtonVisible = Visibility.Hidden;
      ValidateBCodeButtonEnabled = true;
      if (_systemControlSelectorIndex == 0)
        StartButtonEnabled = true;
      else
        StartButtonEnabled = false;

      OrderSelectorState = 0; //Column
      OrderSelectorStateBool = new ObservableCollection<bool> { true, false };
      EndReadSelectorState = new ObservableCollection<bool> { false, false, false };
      EndReadSelector(App.Device.TerminationType);

      Volumes = new ObservableCollection<string> { "0", "", "" };


      var RM = Language.Resources.ResourceManager;
      var curCulture = Language.TranslationSource.Instance.CurrentCulture;
      SpeedItems = new ObservableCollection<DropDownButtonContents>
      {
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Normal), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Hi_Speed), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Hi_Sens), curCulture), this)
      };
      _selectedSpeedIndex = 0;
      SelectedSpeedContent = SpeedItems[_selectedSpeedIndex].Content;
      DropDownButtonContents.ResetIndex();

      ClassiMapItems = new ObservableCollection<DropDownButtonContents>();
      if (App.Device.MapList.Count > 0)
      {
        foreach (var map in App.Device.MapList)
        {
          ClassiMapItems.Add(new DropDownButtonContents(map.mapName, this));
        }
      }
      else
        ClassiMapItems.Add(new DropDownButtonContents("No maps available", this));
      SelectedClassiMapContent = App.Device.ActiveMap.mapName;
      DropDownButtonContents.ResetIndex();

      ChConfigItems = new ObservableCollection<DropDownButtonContents>
      {
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Standard), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Cells), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_FM3D), curCulture), this)
      };
      _selectedChConfigIndex = 0;
      SelectedChConfigContent = ChConfigItems[_selectedChConfigIndex].Content;
      DropDownButtonContents.ResetIndex();

      EventCountField = new ObservableCollection<string> { "" };


      PressureMonToggleButtonState = false;
      PressureMon = new ObservableCollection<string> {"","",""};
      ActiveList = new ObservableCollection<string>();
      Instance = this;
    }

    public static DashboardViewModel Create()
    {
      return ViewModelSource.Create(() => new DashboardViewModel());
    }

    public void PressureMonToggleButtonClick()
    {
      PressureMonToggleButtonState = !PressureMonToggleButtonState;
      MaxPressure = 0;
      MinPressure = 9999999999;
    }

    public void SystemControlSelector(byte num)
    {
      SystemControlSelectorState[0] = false;
      SystemControlSelectorState[1] = false;
      SystemControlSelectorState[2] = false;
      SystemControlSelectorState[num] = true;
      _systemControlSelectorIndex = num;
      App.SetSystemControl(num);
      if (num != 0)
      {
        ExperimentViewModel.Instance.WellSelectVisible = Visibility.Hidden;

        if (num == 2)
        {
          ValidateBCodeButtonVisible = Visibility.Visible;
        }
        else
          ValidateBCodeButtonVisible = Visibility.Hidden;
      }
      else
      {
        ExperimentViewModel.Instance.WellSelectVisible = Visibility.Visible;

        ValidateBCodeButtonVisible = Visibility.Hidden;
      }
    }

    public void OrderSelector(byte num)
    {
      OrderSelectorState = num;
      OrderSelectorStateBool[0] = false;
      OrderSelectorStateBool[1] = false;
      OrderSelectorStateBool[num] = true;
      App.Device.MainCommand("Set Property", code: 0xa8, parameter: num);
    }

    public void EndReadSelector(byte num)
    {
      EndReadSelectorState[0] = false;
      EndReadSelectorState[1] = false;
      EndReadSelectorState[2] = false;
      EndReadSelectorState[num] = true;
      App.SetTerminationType(num);
    }

    public void SetFixedVolumeButtonClick(ushort num)
    {
      App.Device.MainCommand("Set Property", code: 0xaf, parameter: num);
      Volumes[0] = num.ToString();
    }

    public void LoadButtonClick()
    {
      App.Device.MainCommand("Load Plate");
    }

    public void EjectButtonClick()
    {
      App.Device.MainCommand("Eject Plate");
    }

    public void StartButtonClick()
    {
      //read section of plate
      App.Device.ReadActive = true;
      App.Device.MainCommand("Get FProperty", code: 0x58);
      App.Device.MainCommand("Get FProperty", code: 0x68);
      App.Device.PlateReport = new MicroCy.PlateReport(); //TODO: optimize, not needed here
      App.Device.MainCommand("Get FProperty", code: 0x20);   //get high dnr property
      Array.Clear(App.Device.SscData, 0, 256);
      Array.Clear(App.Device.Rp1Data, 0, 256);
      //  chart1.Series["SSC"].Points.DataBindY(m_MicroCy.sscdata);
      //  chart3.Series["RP1"].Points.DataBindY(m_MicroCy.rp1data);
      SetWellsInOrder();

      //find number of wells to read
      if (App.Device.WellsInOrder.Count < 1)
        return;
      //btnEndRead.BackColor = Color.Tomato;  //TODO: ask about it
      StartButtonEnabled = false;

      App.Device.WellsToRead = App.Device.WellsInOrder.Count - 1;    //make zero based like well index is
      App.Device.SetAspirateParamsForWell(0);  //setup for first read
      App.Device.SetReadingParamsForWell(0);
      App.Device.MainCommand("Set Property", code: 0x19, parameter: 1); //bubble detect on
      App.Device.MainCommand("Position Well Plate");   //move motors. next position is set in properties 0xad and 0xae
      App.Device.MainCommand("Aspirate Syringe A"); //handles down and pickup sample
      App.Device.WellNext();   //save well numbers for file neame
      App.Device.InitBeadRead(App.Device.ReadingRow, App.Device.ReadingCol);   //gets output file redy
      App.Device.PrepareSummaryFile(); //TODO : try to move to initbeadread, there is an issue, if during runtime some box is clicked

      if (App.Device.WellsToRead == 0)    //only one well in region
        App.Device.MainCommand("Read A");
      else
      {
        App.Device.SetAspirateParamsForWell(1);
        App.Device.MainCommand("Read A Aspirate B");
      }
      App.Device.CurrentWellIdx = 0;
      if (App.Device.TerminationType != 1)    //set some limit for running to eos or if regions are wrong
        App.Device.BeadsToCapture = 100000;
    }

    public void EndButtonClick()
    {
      if (!App.Device.ReadActive)  //end button press before start, cancel work order
      {
        App.Device.MainCommand("Set Property", code: 0x17); //leds off
      }
      else
      {
        App.Device.EndState = 1;
        if (App.Device.WellsToRead > 0)   //if end read on tube or single well, nothing else is aspirated otherwise
          App.Device.WellsToRead = App.Device.CurrentWellIdx + 1; //just read the next well in order since it is already aspirated
      }
    }

    public void ValidateBCodeButtonClick()
    {
      App.Device.MainCommand("Set Property", code: 0xad, parameter: 1); //move plate to best image distance
      App.Device.ReadingRow = MotorsViewModel.Instance.RowColIndex.rowIndex;
      App.Device.MainCommand("Set Property", code: 0xae);
      App.Device.ReadingCol = MotorsViewModel.Instance.RowColIndex.colIndex;
      App.Device.MainCommand("Set Property", code: 0x17, parameter: 1); //leds on
      App.Device.MainCommand("Position Well Plate");
      //if (videogoing == false)
      //{
      //  FinalVideo = new VideoCaptureDevice(VideoCaptureDevices[comboBox1.SelectedIndex].MonikerString);
      //  FinalVideo.VideoResolution = FinalVideo.VideoCapabilities[7]; //It selects the default size
      //  FinalVideo.NewFrame += new NewFrameEventHandler(FinalVideo_NewFrame);
      //  videogoing = true;
      //}
      //
      //FinalVideo.Start();
    }

    public void FocusedBox(int num)
    {
      switch (num)
      {
        case 0:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(EndRead)), this, 0);
          break;
        case 1:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(EndRead)), this, 1);
          break;
        case 2:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(Volumes)), this, 0);
          break;
        case 3:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(Volumes)), this, 1);
          break;
        case 4:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(Volumes)), this, 2);
          break;
      }
    }


    private MicroCy.Wells MakeWell(byte row, byte col)
    {
      MicroCy.Wells newwell = new MicroCy.Wells();
      newwell.rowIdx = row;
      newwell.colIdx = col;
      newwell.runSpeed = _selectedSpeedIndex;
      short sRes = 0;
      _ = short.TryParse(Volumes[0], out sRes);
      newwell.sampVol = sRes;
      sRes = 0;
      _ = short.TryParse(Volumes[1], out sRes);
      newwell.washVol = sRes;
      sRes = 0;
      _ = short.TryParse(Volumes[2], out sRes);
      newwell.agitateVol = sRes;
      newwell.termType = App.Device.TerminationType;
      newwell.chanConfig = _selectedChConfigIndex;
      newwell.regTermCnt = App.Device.MinPerRegion;
      newwell.termCnt = App.Device.BeadsToCapture;
      newwell.thisWellsMap = App.Device.ActiveMap;
      return newwell;
    }

    private void SetWellsInOrder()
    {
      App.Device.WellsInOrder.Clear();
      if (WellsSelectViewModel.Instance.CurrentTableSize > 1)    //TODO: platetype can be removed from device fields, as soon as workorder stuff is done
      {
        ObservableCollection<WellTableRow> plate = WellsSelectViewModel.Instance.CurrentTableSize == 96 ?
          WellsSelectViewModel.Instance.Table96Wells : WellsSelectViewModel.Instance.Table384Wells;
        if (_systemControlSelectorIndex == 0)  //manual control of plate //TODO: SystemControl can be removed from device fields maybe?
        {
          for (byte r = 0; r < plate.Count; r++)
          {
            for (byte c = 0; c < plate[r].Types.Count; c++)
            {
              if (plate[r].Types[c] != WellType.Empty)
                App.Device.WellsInOrder.Add(MakeWell(r, c));
            }
          }
          if (OrderSelectorState == 0)
          {
            //sort list by col/row
            App.Device.WellsInOrder.Sort((x, y) => x.colIdx.CompareTo(y.colIdx));
          }
        }
        else    //Work Order control of plate
        {
          //fill wells from work order
          App.Device.WellsInOrder = App.Device.WorkOrder.woWells;
        }
        App.Device.WellsToRead = App.Device.WellsInOrder.Count;
      }
      else if (WellsSelectViewModel.Instance.CurrentTableSize == 1)  //tube
        App.Device.WellsInOrder.Add(MakeWell(0, 0));    //a 1 record work order

    }

    public class DropDownButtonContents : Core.ObservableObject
    {
      public string Content
      {
        get => _content;
        set
        {
          _content = value;
          OnPropertyChanged();
        }
      }
      public byte Index { get; set; }
      private static byte _nextIndex = 0;
      private string _content;
      private static DashboardViewModel _vm;
      public DropDownButtonContents(string content, DashboardViewModel vm = null)
      {
        if (_vm == null)
        {
          _vm = vm;
        }
        Content = content;
        Index = _nextIndex++;
      }

      public void Click(int num)
      {
        switch (num)
        {
          case 1:
            _vm.SelectedSpeedContent = Content;
            _vm._selectedSpeedIndex = Index;
            App.Device.MainCommand("Set Property", code: 0xaa, parameter: (ushort)Index);
            break;
          case 2:
            _vm.SelectedClassiMapContent = Content;
            App.SetActiveMap(Content);
            App.Device.MainCommand("Set Property", code: 0xa9, parameter: (ushort)Index);
            break;
          case 3:
            _vm.SelectedChConfigContent = Content;
            _vm._selectedChConfigIndex = Index;
            App.Device.MainCommand("Set Property", code: 0xc2, parameter: (ushort)Index);
            break;
        }
      }

      public void ForAppUpdater(int num)
      {
        switch (num)
        {
          case 1:
            _vm.SelectedSpeedContent = Content;
            _vm._selectedSpeedIndex = Index;
            break;
          case 2:
            _vm.SelectedClassiMapContent = Content;
            break;
          case 3:
            _vm.SelectedChConfigContent = Content;
            _vm._selectedChConfigIndex = Index;
            break;
        }
      }

      public static void ResetIndex()
      {
        _nextIndex = 0;
      }
    }
  }
}