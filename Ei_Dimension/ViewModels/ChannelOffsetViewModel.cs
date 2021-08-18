using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ChannelOffsetViewModel
  {
    public virtual string[] ChannelsOffsetParameters { get; set; }
    protected ChannelOffsetViewModel()
    {
      ChannelsOffsetParameters = new string[10];
    }

    public static ChannelOffsetViewModel Create()
    {
      return ViewModelSource.Create(() => new ChannelOffsetViewModel());
    }
  }
}