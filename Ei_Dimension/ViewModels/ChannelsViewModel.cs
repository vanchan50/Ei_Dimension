using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ChannelsViewModel
  {
    protected ChannelsViewModel()
    {
    }

    public static ChannelsViewModel Create()
    {
      return ViewModelSource.Create(() => new ChannelsViewModel());
    }
  }
}