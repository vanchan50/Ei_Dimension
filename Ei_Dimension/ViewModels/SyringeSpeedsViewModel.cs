﻿using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class SyringeSpeedsViewModel
  {
    public virtual ObservableCollection<string> SheathSyringeParameters { get; set; }
    public virtual ObservableCollection<string> SamplesSyringeParameters { get; set; }
    public static SyringeSpeedsViewModel Instance { get; private set; }

    protected SyringeSpeedsViewModel()
    {
      SheathSyringeParameters = new ObservableCollection<string>();
      SamplesSyringeParameters = new ObservableCollection<string>();
      for (var i = 0; i < 6; i++)
      {
        SheathSyringeParameters.Add("");
        SamplesSyringeParameters.Add("");
      }
      Instance = this;
    }

    public static SyringeSpeedsViewModel Create()
    {
      return ViewModelSource.Create(() => new SyringeSpeedsViewModel());
    }

    public void FocusedBox(int num)
    {
      switch (num)
      {
        case 0:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(SheathSyringeParameters)), this, 0);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.sheathSP.Children[0]);
          break;
        case 1:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(SheathSyringeParameters)), this, 1);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.sheathSP.Children[1]);
          break;
        case 2:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(SheathSyringeParameters)), this, 2);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.sheathSP.Children[2]);
          break;
        case 3:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(SheathSyringeParameters)), this, 3);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.sheathSP.Children[3]);
          break;
        case 4:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(SheathSyringeParameters)), this, 4);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.sheathSP.Children[4]);
          break;
        case 5:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(SheathSyringeParameters)), this, 5);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.sheathSP.Children[5]);
          break;
        case 6:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(SamplesSyringeParameters)), this, 0);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.samplesSP.Children[0]);
          break;
        case 7:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(SamplesSyringeParameters)), this, 1);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.samplesSP.Children[1]);
          break;
        case 8:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(SamplesSyringeParameters)), this, 2);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.samplesSP.Children[2]);
          break;
        case 9:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(SamplesSyringeParameters)), this, 3);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.samplesSP.Children[3]);
          break;
        case 10:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(SamplesSyringeParameters)), this, 4);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.samplesSP.Children[4]);
          break;
        case 11:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(SamplesSyringeParameters)), this, 5);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.samplesSP.Children[5]);
          break;
      }
    }

    public void TextChanged(TextChangedEventArgs e)
    {
      App.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
    }
  }
}