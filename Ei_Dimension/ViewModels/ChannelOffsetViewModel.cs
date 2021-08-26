using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ChannelOffsetViewModel
  {
    public virtual ObservableCollection<string> ChannelsOffsetParameters { get; set; }
    protected ChannelOffsetViewModel()
    {
      ChannelsOffsetParameters = new ObservableCollection<string>();
      for (var i = 0; i < 10; i++)
      {
        ChannelsOffsetParameters.Add("");
      }
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
          break;
        case 1:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 1);
          break;
        case 2:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 2);
          break;
        case 3:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 3);
          break;
        case 4:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 4);
          break;
        case 5:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 5);
          break;
        case 6:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 6);
          break;
        case 7:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 7);
          break;
        case 8:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 8);
          break;
        case 9:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ChannelsOffsetParameters)), this, 9);
          break;
      }
    }
  }
}