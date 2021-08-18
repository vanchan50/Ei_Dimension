using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ChannelOffsetViewModel
  {
    protected ChannelOffsetViewModel()
    {
    }

    public static ChannelOffsetViewModel Create()
    {
      return ViewModelSource.Create(() => new ChannelOffsetViewModel());
    }
  }
}