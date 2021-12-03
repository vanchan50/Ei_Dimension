using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ChannelOffsetViewModel
  {
    public virtual ObservableCollection<string> ChannelsOffsetParameters { get; set; }
    public virtual System.Windows.Visibility OldBoardOffsetsVisible { get; set; }
    public virtual ObservableCollection<string> SiPMTempCoeff { get; set; }
    public virtual string SelectedSensitivityContent { get; set; }
    public virtual ObservableCollection<DropDownButtonContents> SensitivityItems { get; set; }
    public byte SelectedSensitivityIndex { get; set; }
    public static ChannelOffsetViewModel Instance { get; private set; }
    

    protected ChannelOffsetViewModel()
    {
      ChannelsOffsetParameters = new ObservableCollection<string>();
      OldBoardOffsetsVisible = System.Windows.Visibility.Visible;
      SiPMTempCoeff = new ObservableCollection<string> { "" };
      for (var i = 0; i < 10; i++)
      {
        ChannelsOffsetParameters.Add("");
      }
      var RM = Language.Resources.ResourceManager;
      var curCulture = Language.TranslationSource.Instance.CurrentCulture;
      SensitivityItems = new ObservableCollection<DropDownButtonContents>
      {
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Channels_Sens_B), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Channels_Sens_C), curCulture), this)
      };
      SelectedSensitivityIndex = Settings.Default.SensitivityChannelB ? (byte)0 : (byte)1;
      SelectedSensitivityContent = SensitivityItems[SelectedSensitivityIndex].Content;
      Instance = this;
    }

    public static ChannelOffsetViewModel Create()
    {
      return ViewModelSource.Create(() => new ChannelOffsetViewModel());
    }

    public void FocusedBox(int num)
    {
      switch (num)
      {
        case 0:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 0);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelOffsetView.Instance.SP.Children[0]);
          break;
        case 1:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 1);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelOffsetView.Instance.SP.Children[1]);
          break;
        case 2:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 2);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelOffsetView.Instance.SP.Children[2]);
          break;
        case 3:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 3);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelOffsetView.Instance.SP.Children[3]);
          break;
        case 4:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 4);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelOffsetView.Instance.SP.Children[4]);
          break;
        case 5:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 5);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelOffsetView.Instance.SP.Children[5]);
          break;
        case 6:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 6);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelOffsetView.Instance.SP.Children[6]);
          break;
        case 7:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 7);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelOffsetView.Instance.SP.Children[7]);
          break;
        case 8:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 8);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelOffsetView.Instance.SP.Children[8]);
          break;
        case 9:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 9);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.ChannelOffsetView.Instance.SP.Children[9]);
          break;
        case 10:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(SiPMTempCoeff)), this, 0);
          MainViewModel.Instance.NumpadToggleButton(Views.ChannelOffsetView.Instance.CoefTB);
          break;
      }
    }

    public void TextChanged(TextChangedEventArgs e)
    {
      App.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
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
      private static ChannelOffsetViewModel _vm;
      public DropDownButtonContents(string content, ChannelOffsetViewModel vm = null)
      {
        if (_vm == null)
        {
          _vm = vm;
        }
        Content = content;
        Index = _nextIndex++;
      }

      public void Click()
      {
        _vm.SelectedSensitivityContent = Content;
        _vm.SelectedSensitivityIndex = Index;
        App.SetSensitivityChannel(Index);
      }

      public static void ResetIndex()
      {
        _nextIndex = 0;
      }
    }
  }
}