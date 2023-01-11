using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;
using System.Threading.Tasks;
using System.Windows;
using Ei_Dimension.Controllers;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class SelRegionsViewModel
  {
    public static SelRegionsViewModel Instance { get; private set; }
    public virtual Visibility WaitIndicatorBorderVisibility { get; set; }

    protected SelRegionsViewModel()
    {
      WaitIndicatorBorderVisibility = Visibility.Hidden;
      Instance = this;
    }

    public static SelRegionsViewModel Create()
    {
      return ViewModelSource.Create(() => new SelRegionsViewModel());
    }

    public void AddActiveRegion(byte num)
    {
      //App.MapRegions.AddActiveRegion();
    }

    public void AllSelectClick()
    {
      ShowWaitIndicator();
      Task.Run(() =>
      {
        App.Current.Dispatcher.Invoke((Action)
          (() =>
          {
            UserInputHandler.InputSanityCheck();
            for (var i = 0; i < MapRegionsController.RegionsList.Count; i++)
            {
              var reg = MapRegionsController.RegionsList[i].Number;
              if(reg == 0)
                continue;
              if (!MapRegionsController.ActiveRegionNums.Contains(reg))
                App.MapRegions.AddActiveRegion(reg);
            }

            HideWaitIndicator();
          }));
      });
    }

    public void ResetClick()
    {
      ShowWaitIndicator();
      Task.Run(() =>
      {
        App.Current.Dispatcher.Invoke((Action)
          (() =>
          {
            UserInputHandler.InputSanityCheck();
            App.MapRegions.ResetRegions();
            HideWaitIndicator();
          }));
      });
    }

    private void ShowWaitIndicator()
    {
      WaitIndicatorBorderVisibility = Visibility.Visible;
    }

    private void HideWaitIndicator()
    {
      WaitIndicatorBorderVisibility = Visibility.Hidden;
    }
  }
}