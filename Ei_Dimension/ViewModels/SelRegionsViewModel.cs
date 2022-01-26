using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class SelRegionsViewModel
  {
    public static SelRegionsViewModel Instance { get; private set; }
    public virtual Visibility WaitIndicatorBorderVisibility { get; set; }
    public virtual bool WaitIndicatorVisibility { get; set; }

    protected SelRegionsViewModel()
    {
      WaitIndicatorBorderVisibility = Visibility.Hidden;
      WaitIndicatorVisibility = false;
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
            for (var i = 1; i < App.MapRegions.ActiveRegions.Count; i++)
            {
              if (!App.MapRegions.ActiveRegions[i])
                App.MapRegions.AddActiveRegion(i);
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
            App.MapRegions.FillRegions();
            HideWaitIndicator();
          }));
      });
    }

    private void ShowWaitIndicator()
    {
      WaitIndicatorBorderVisibility = Visibility.Visible;
      WaitIndicatorVisibility = true;
    }

    private void HideWaitIndicator()
    {
      WaitIndicatorBorderVisibility = Visibility.Hidden;
      WaitIndicatorVisibility = false;
    }
  }
}