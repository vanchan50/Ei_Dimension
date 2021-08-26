using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class NumpadViewModel
  {
    protected NumpadViewModel()
    {

    }

    public static NumpadViewModel Create()
    {
      return ViewModelSource.Create(() => new NumpadViewModel());
    }

    public void NumpadInputClick(int n)
    {
      switch (n)
      {
        case 0:
          App.InjectToFocusedTextbox("0");
          break;
        case 1:
          App.InjectToFocusedTextbox("1");
          break;
        case 2:
          App.InjectToFocusedTextbox("2");
          break;
        case 3:
          App.InjectToFocusedTextbox("3");
          break;
        case 4:
          App.InjectToFocusedTextbox("4");
          break;
        case 5:
          App.InjectToFocusedTextbox("5");
          break;
        case 6:
          App.InjectToFocusedTextbox("6");
          break;
        case 7:
          App.InjectToFocusedTextbox("7");
          break;
        case 8:
          App.InjectToFocusedTextbox("8");
          break;
        case 9:
          App.InjectToFocusedTextbox("9");
          break;
        case 10:
          App.InjectToFocusedTextbox("");
          break;
      }
    }
  }
}