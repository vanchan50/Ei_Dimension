using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class MotorsViewModel
  {
    public virtual bool PollStepActive { get; set; }
    public virtual ObservableCollection<DropDownButtonContents> WellRowButtonItems { get; set; }
    public virtual ObservableCollection<DropDownButtonContents> WellColumnButtonItems { get; set; }
    public virtual string SelectedWellRow { get; set; }
    public virtual string SelectedWellColumn { get; set; }
    public virtual string[] ParametersX { get; set; }
    public virtual string[] ParametersY { get; set; }
    public virtual string[] ParametersZ { get; set; }
    public virtual string[] StepsParametersX { get; set; }
    public virtual string[] StepsParametersY { get; set; }
    public virtual string[] StepsParametersZ { get; set; }


    private int _amountOfWells;

    protected MotorsViewModel()
    {
      PollStepActive = false;
      ParametersX = new string[7];
      ParametersY = new string[7];
      ParametersZ = new string[7];
      StepsParametersX = new string[5];
      StepsParametersY = new string[5];
      StepsParametersZ = new string[5];
      ParametersX[1] = "Left";
      ParametersY[1] = "Front";
      ParametersZ[1] = "Up";
      _amountOfWells = 96;
      SelectedWellRow = "A";
      SelectedWellColumn = "1";
      WellRowButtonItems = new ObservableCollection<DropDownButtonContents> { new DropDownButtonContents("A", this) };
      for (var i = 1; i < 8; i++)
      {
        WellRowButtonItems.Add(new DropDownButtonContents(Convert.ToChar('A' + i).ToString()));
      }
      WellColumnButtonItems = new ObservableCollection<DropDownButtonContents>();
      for (var i = 1; i < 13; i++)
      {
        WellColumnButtonItems.Add(new DropDownButtonContents(i.ToString()));
      }
    }

    public static MotorsViewModel Create()
    {
      return ViewModelSource.Create(() => new MotorsViewModel());
    }

    public void ChangeAmountOfWells(int num)
    {
      _amountOfWells = num;
      WellRowButtonItems.Clear();
      WellColumnButtonItems.Clear();
      switch (num)
      {
        case 96:
          WellRowButtonItems = new ObservableCollection<DropDownButtonContents> { new DropDownButtonContents("A", this) };
          for (var i = 1; i < 8; i++)
          {
            WellRowButtonItems.Add(new DropDownButtonContents(Convert.ToChar('A' + i).ToString()));
          }
          WellColumnButtonItems = new ObservableCollection<DropDownButtonContents>();
          for (var i = 1; i < 13; i++)
          {
            WellColumnButtonItems.Add(new DropDownButtonContents(i.ToString()));
          }
          break;
        case 384:
          WellRowButtonItems = new ObservableCollection<DropDownButtonContents> { new DropDownButtonContents("A", this) };
          for (var i = 1; i < 16; i++)
          {
            WellRowButtonItems.Add(new DropDownButtonContents(Convert.ToChar('A' + i).ToString()));
          }
          WellColumnButtonItems = new ObservableCollection<DropDownButtonContents>();
          for (var i = 1; i < 25; i++)
          {
            WellColumnButtonItems.Add(new DropDownButtonContents(i.ToString()));
          }
          break;
      }
    }

    public void RunMotorButtonClick(string s)
    {
      switch (s) { 
        case "x":
          break;
        case "y":
          break;
        case "z":
          break;
      }
    }

    public void HaltMotorButtonClick(string s)
    {
      switch (s)
      {
        case "x":
          break;
        case "y":
          break;
        case "z":
          break;
      }
    }

    public void GoToWellButtonClick()
    {

    }

    public void PollStepToggleButtonClick()
    {
      PollStepActive = !PollStepActive;
    }

    public void PollStepSelector(string s)
    {
      switch (s)
      {
        case "Left":
          ParametersX[1] = s;
          break;
        case "Right":
          ParametersX[1] = s;
          break;
        case "Back":
          ParametersY[1] = s;
          break;
        case "Front":
          ParametersY[1] = s;
          break;
        case "Up":
          ParametersZ[1] = s;
          break;
        case "Down":
          ParametersZ[1] = s;
          break;
      }
    }


    public class DropDownButtonContents
    {
      public string Content { get; set; }
      private static MotorsViewModel _vm;
      public DropDownButtonContents(string content, MotorsViewModel vm = null)
      {
        if (_vm == null)
        {
          _vm = vm;
        }
        Content = content;
      }

      public void Click(int num)
      {
        switch (num)
        {
          case 1:
            _vm.SelectedWellRow = Content;
            break;
          case 2:
            _vm.SelectedWellColumn = Content;
            break;
        }
      }
    }
  }

  
}