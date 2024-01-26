using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using DIOS.Core;
using Ei_Dimension.Controllers;

namespace Ei_Dimension.ViewModels;

[POCOViewModel]
public class VerificationParametersViewModel
{
  public virtual ObservableCollection<string> ToleranceItems { get; set; } = new(){ "", "", "", "", "" };
  public virtual ObservableCollection<string> MaxCVItems { get; set; } = new(){ "", "", "", "", "" };
  public virtual ObservableCollection<string> TargetReporter { get; set; } = new(){ "" };
  public virtual ObservableCollection<string> SelectedRegionNum { get; set; } = new(){ "" };
  public virtual ObservableCollection<bool> IsActiveCheckbox { get; set; } = new(){ false };
  public static VerificationParametersViewModel Instance { get; private set; }
  public MapRegion CurrentRegion { get; private set; }

  protected VerificationParametersViewModel()
  {
    Instance = this;
  }

  public static VerificationParametersViewModel Create()
  {
    return ViewModelSource.Create(() => new VerificationParametersViewModel());
  }

  public void InstallRegionVerificationData(in MapRegion region)
  {
    VerificationViewModel.Instance.DetailsVisibility = Visibility.Visible;
    CurrentRegion = region;
    ToleranceItems[0] = region.MeanTolerance.GreenSSC.ToString("F1");
    ToleranceItems[1] = region.MeanTolerance.RedSSC.ToString("F1");
    ToleranceItems[2] = region.MeanTolerance.Cl1.ToString("F1");
    ToleranceItems[3] = region.MeanTolerance.Cl2.ToString("F1");
    ToleranceItems[4] = region.MeanTolerance.Reporter.ToString("F1");

    MaxCVItems[0] = region.MaxCV.GreenSSC.ToString("F1");
    MaxCVItems[1] = region.MaxCV.RedSSC.ToString("F1");
    MaxCVItems[2] = region.MaxCV.Cl1.ToString("F1");
    MaxCVItems[3] = region.MaxCV.Cl2.ToString("F1");
    MaxCVItems[4] = region.MaxCV.Reporter.ToString("F1");
    TargetReporter[0] = region.VerificationTargetReporter.ToString("F1");
    SelectedRegionNum[0] = region.Number.ToString();
    if (region.isValidator)
    {
      IsActiveCheckedBox(true);
    }
    else
    {
      IsActiveUncheckedBox(true);
    }
    //IsActiveCheckbox[0] = region.isValidator;
  }


  public void IsActiveCheckedBox(bool fromCode = false)
  {
    UserInputHandler.InputSanityCheck();
    IsActiveCheckbox[0] = true;
    CurrentRegion.isValidator = true;
    App.MapRegions.ActivateVerificationTextBox(CurrentRegion.Number, true);
  }

  public void IsActiveUncheckedBox(bool fromCode = false)
  {
    UserInputHandler.InputSanityCheck();
    IsActiveCheckbox[0] = false;
    CurrentRegion.isValidator = false;
    App.MapRegions.ActivateVerificationTextBox(CurrentRegion.Number, false);
  }

  public void TextChanged(TextChangedEventArgs e)
  {
    UserInputHandler.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
  }

  public void FocusedBox(int num)
  {
    var toleranceSP = Views.VerificationParametersView.Instance.toleranceSP.Children;
    var maxCV_SP = Views.VerificationParametersView.Instance.maxCV_SP.Children;
    var reporterTb = Views.VerificationParametersView.Instance.TargetReporterTb;
    switch (num)
    {
      case 0:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ToleranceItems)), this, 0, toleranceSP[0] as TextBox);
        MainViewModel.Instance.NumpadToggleButton(toleranceSP[0] as TextBox);
        break;
      case 1:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ToleranceItems)), this, 1, toleranceSP[1] as TextBox);
        MainViewModel.Instance.NumpadToggleButton(toleranceSP[1] as TextBox);
        break;
      case 2:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ToleranceItems)), this, 2, toleranceSP[2] as TextBox);
        MainViewModel.Instance.NumpadToggleButton(toleranceSP[2] as TextBox);
        break;
      case 3:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ToleranceItems)), this, 3, toleranceSP[3] as TextBox);
        MainViewModel.Instance.NumpadToggleButton(toleranceSP[3] as TextBox);
        break;
      case 4:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ToleranceItems)), this, 4, toleranceSP[4] as TextBox);
        MainViewModel.Instance.NumpadToggleButton(toleranceSP[4] as TextBox);
        break;
      case 5:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(MaxCVItems)), this, 0, maxCV_SP[0] as TextBox);
        MainViewModel.Instance.NumpadToggleButton(maxCV_SP[0] as TextBox);
        break;
      case 6:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(MaxCVItems)), this, 1, maxCV_SP[1] as TextBox);
        MainViewModel.Instance.NumpadToggleButton(maxCV_SP[1] as TextBox);
        break;
      case 7:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(MaxCVItems)), this, 2, maxCV_SP[2] as TextBox);
        MainViewModel.Instance.NumpadToggleButton(maxCV_SP[2] as TextBox);
        break;
      case 8:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(MaxCVItems)), this, 3, maxCV_SP[3] as TextBox);
        MainViewModel.Instance.NumpadToggleButton(maxCV_SP[3] as TextBox);
        break;
      case 9:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(MaxCVItems)), this, 4, maxCV_SP[4] as TextBox);
        MainViewModel.Instance.NumpadToggleButton(maxCV_SP[4] as TextBox);
        break;
      case 10:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(TargetReporter)), this, 0, reporterTb);
        MainViewModel.Instance.NumpadToggleButton(reporterTb);
        break;
    }
  }
}