
namespace Ei_Dimension.Models
{
  public class DropdownStrings : Core.ObservableObject
  {
    public string Content { get; set; }
    public DropdownStrings(string s)
    {
      Content = s;
    }
  }
}
