﻿using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Ei_Dimension.ViewModels;

[POCOViewModel]
public class AlignmentViewModel
{
  public virtual ObservableCollection<string> VerificationTolerances { get; set; }
  public static AlignmentViewModel Instance { get; private set; }

  protected AlignmentViewModel()
  {
    VerificationTolerances = new ObservableCollection<string>
    {
      Settings.Default.ValidatorToleranceReporter.ToString("0.000"),
      Settings.Default.ValidatorToleranceClassification.ToString("0.000"),
      Settings.Default.ValidatorToleranceMisclassification.ToString("0.000")
    };
    Instance  = this;
  }

  public static AlignmentViewModel Create()
  {
    return ViewModelSource.Create(() => new AlignmentViewModel());
  }

  public void TextChanged(TextChangedEventArgs e)
  {
    UserInputHandler.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
  }

  public void FocusedBox(int num)
  {
    var Stackpanel = Views.AlignmentView.Instance.ToleranceSP.Children;
    switch (num)
    {
      case 0:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(VerificationTolerances)), this, 0, (TextBox)Stackpanel[1]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Stackpanel[1]);
        break;
      case 1:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(VerificationTolerances)), this, 1, (TextBox)Stackpanel[3]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Stackpanel[3]);
        break;
      case 2:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(VerificationTolerances)), this, 2, (TextBox)Stackpanel[5]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Stackpanel[5]);
        break;
    }
  }
}