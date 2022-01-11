using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ChannelsViewModel
  {
    public virtual ObservableCollection<string> Bias30Parameters { get; set; }
    public virtual ObservableCollection<string> TcompBiasParameters { get; set; }
    public virtual ObservableCollection<string> TempParameters { get; set; }
    public virtual ObservableCollection<string> BackgroundParameters { get; set; }

    public static ChannelsViewModel Instance { get; private set; }

    protected ChannelsViewModel()
    {
      Bias30Parameters = new ObservableCollection<string>();
      Bias30Parameters.Add(App.Device.ActiveMap.calgssc.ToString());
      Bias30Parameters.Add(App.Device.ActiveMap.calrpmaj.ToString());
      Bias30Parameters.Add(App.Device.ActiveMap.calrpmin.ToString());
      Bias30Parameters.Add(App.Device.ActiveMap.calcl3.ToString());
      Bias30Parameters.Add(App.Device.ActiveMap.calrssc.ToString());
      Bias30Parameters.Add(App.Device.ActiveMap.calcl1.ToString());
      Bias30Parameters.Add(App.Device.ActiveMap.calcl2.ToString());
      Bias30Parameters.Add(App.Device.ActiveMap.calvssc.ToString());
      Bias30Parameters.Add(App.Device.ActiveMap.calcl0.ToString());
      Bias30Parameters.Add(App.Device.ActiveMap.calfsc.ToString());
      TcompBiasParameters = new ObservableCollection<string>();
      TempParameters = new ObservableCollection<string>();
      BackgroundParameters = new ObservableCollection<string>();
      for(var i = 0; i < 10; i++)
      {
        TcompBiasParameters.Add("");
        TempParameters.Add("");
        BackgroundParameters.Add("");
      }
      Instance = this;
    }

    public static ChannelsViewModel Create()
    {
      return ViewModelSource.Create(() => new ChannelsViewModel());
    }

    public void UpdateBiasButtonClick()
    {
      UserInputHandler.InputSanityCheck();
      App.Device.MainCommand("RefreshDac");
      App.Device.InitSTab("channeltab");
    }

    public void SaveBiasButtonClick()
    {
      UserInputHandler.InputSanityCheck();
      App.Device.SaveCalVals(new MicroCy.MapCalParameters
      {
        TempCl0 = int.Parse(Bias30Parameters[8]),
        TempCl1 = int.Parse(Bias30Parameters[5]),
        TempCl2 = int.Parse(Bias30Parameters[6]),
        TempCl3 = int.Parse(Bias30Parameters[3]),
        TempRedSsc = int.Parse(Bias30Parameters[4]),
        TempGreenSsc = int.Parse(Bias30Parameters[0]),
        TempVioletSsc = int.Parse(Bias30Parameters[7]),
        TempRpMaj = int.Parse(Bias30Parameters[1]),
        TempRpMin = int.Parse(Bias30Parameters[2]),
        TempFsc = int.Parse(Bias30Parameters[9]),
        Compensation = float.Parse(CalibrationViewModel.Instance.CompensationPercentageContent[0]),
        Gating = CalibrationViewModel.Instance.SelectedGatingIndex,
        Height = short.Parse(CalibrationViewModel.Instance.EventTriggerContents[0]),
        MinSSC = ushort.Parse(CalibrationViewModel.Instance.EventTriggerContents[1]),
        MaxSSC = ushort.Parse(CalibrationViewModel.Instance.EventTriggerContents[2]),
        DNRCoef = float.Parse(CalibrationViewModel.Instance.DNRContents[0]),
        DNRTrans = float.Parse(CalibrationViewModel.Instance.DNRContents[1]),
        Attenuation = int.Parse(CalibrationViewModel.Instance.AttenuationBox[0]),
        CL0 = int.Parse(CalibrationViewModel.Instance.ClassificationTargetsContents[0]),
        CL1 = int.Parse(CalibrationViewModel.Instance.ClassificationTargetsContents[1]),
        CL2 = int.Parse(CalibrationViewModel.Instance.ClassificationTargetsContents[2]),
        CL3 = int.Parse(CalibrationViewModel.Instance.ClassificationTargetsContents[3]),
        RP1 = int.Parse(CalibrationViewModel.Instance.ClassificationTargetsContents[4]),
        Caldate = null,
        Valdate = null
      });
      App.ShowNotification($"Calibration Parameters Saved to map {App.Device.ActiveMap.mapName}");
    }

    public void FocusedBox(int num)
    {
      switch (num)
      {
        case 0:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 0, (TextBox)Views.ChannelsView.Instance.SP.Children[0]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelsView.Instance.SP.Children[0]);
          break;
        case 1:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 1, (TextBox)Views.ChannelsView.Instance.SP.Children[1]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelsView.Instance.SP.Children[1]);
          break;
        case 2:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 2, (TextBox)Views.ChannelsView.Instance.SP.Children[2]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelsView.Instance.SP.Children[2]);
          break;
        case 3:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 3, (TextBox)Views.ChannelsView.Instance.SP.Children[3]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelsView.Instance.SP.Children[3]);
          break;
        case 4:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 4, (TextBox)Views.ChannelsView.Instance.SP.Children[4]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelsView.Instance.SP.Children[4]);
          break;
        case 5:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 5, (TextBox)Views.ChannelsView.Instance.SP.Children[5]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelsView.Instance.SP.Children[5]);
          break;
        case 6:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 6, (TextBox)Views.ChannelsView.Instance.SP.Children[6]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelsView.Instance.SP.Children[6]);
          break;
        case 7:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 7, (TextBox)Views.ChannelsView.Instance.SP.Children[7]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelsView.Instance.SP.Children[7]);
          break;
        case 8:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 8, (TextBox)Views.ChannelsView.Instance.SP.Children[8]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelsView.Instance.SP.Children[8]);
          break;
        case 9:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(Bias30Parameters)), this, 9, (TextBox)Views.ChannelsView.Instance.SP.Children[9]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelsView.Instance.SP.Children[9]);
          break;
      }
    }

    public void TextChanged(TextChangedEventArgs e)
    {
      UserInputHandler.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
    }

  }
}