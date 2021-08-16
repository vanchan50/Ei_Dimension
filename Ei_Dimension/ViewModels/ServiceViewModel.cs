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
  public class ServiceViewModel
  {
    public virtual List<DropDownButtonContents> WellRowButtonItems { get; set; }
    public virtual List<DropDownButtonContents> WellColumnButtonItems { get; set; }
    public virtual string WellRowButtonContent { get; set; }
    public virtual string WellColumnButtonContent { get; set; }

    protected ServiceViewModel()
    {
      WellRowButtonContent = "A";
      WellColumnButtonContent = "1";
      WellRowButtonItems = new List<DropDownButtonContents> { new DropDownButtonContents("A", this) };
      for (var i = 1; i < 8; i++)
      {
        WellRowButtonItems.Add(new DropDownButtonContents(Convert.ToChar('A' + i).ToString()));
      }
      WellColumnButtonItems = new List<DropDownButtonContents>();
      for (var i = 1; i < 13; i++)
      {
        WellColumnButtonItems.Add(new DropDownButtonContents(i.ToString()));
      }
    }

    public static ServiceViewModel Create()
    {
      return ViewModelSource.Create(() => new ServiceViewModel());
    }

    public void WellRowButtonClick()
    {

    }
    public void WellColumnButtonClick()
    {

    }
  }

  public class DropDownButtonContents
  {
    public string Content { get; set; }
    private static ServiceViewModel _vm;
    public DropDownButtonContents(string content, ServiceViewModel vm = null)
    {
      if(_vm == null)
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
          _vm.WellRowButtonContent = Content;
          _vm.WellRowButtonClick();
          break;
        case 2:
          _vm.WellColumnButtonContent = Content;
          _vm.WellColumnButtonClick();
          break;
      }
    }
  }
}