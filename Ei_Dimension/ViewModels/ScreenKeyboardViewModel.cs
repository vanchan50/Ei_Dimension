using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ScreenKeyboardViewModel
  {
    protected ScreenKeyboardViewModel()
    {

    }

    public static ScreenKeyboardViewModel Create()
    {
      return ViewModelSource.Create(() => new ScreenKeyboardViewModel());
    }

    public void KeyboardInputClick(int n)
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
        case 11:
          App.InjectToFocusedTextbox(".");
          break;
        case 12:
          App.InjectToFocusedTextbox("-");
          break;
        case 13:
          App.InjectToFocusedTextbox("+");
          break;
        case 14:
          App.InjectToFocusedTextbox(" ");
          break;
        case 15:
          App.InjectToFocusedTextbox("(");
          break;
        case 16:
          App.InjectToFocusedTextbox(")");
          break;
        case 17:
          App.InjectToFocusedTextbox("q");
          break;
        case 18:
          App.InjectToFocusedTextbox("w");
          break;
        case 19:
          App.InjectToFocusedTextbox("e");
          break;
        case 20:
          App.InjectToFocusedTextbox("r");
          break;
        case 21:
          App.InjectToFocusedTextbox("t");
          break;
        case 22:
          App.InjectToFocusedTextbox("y");
          break;
        case 23:
          App.InjectToFocusedTextbox("u");
          break;
        case 24:
          App.InjectToFocusedTextbox("i");
          break;
        case 25:
          App.InjectToFocusedTextbox("o");
          break;
        case 26:
          App.InjectToFocusedTextbox("p");
          break;
        case 27:
          App.InjectToFocusedTextbox("a");
          break;
        case 28:
          App.InjectToFocusedTextbox("s");
          break;
        case 29:
          App.InjectToFocusedTextbox("d");
          break;
        case 30:
          App.InjectToFocusedTextbox("f");
          break;
        case 31:
          App.InjectToFocusedTextbox("g");
          break;
        case 32:
          App.InjectToFocusedTextbox("h");
          break;
        case 33:
          App.InjectToFocusedTextbox("j");
          break;
        case 34:
          App.InjectToFocusedTextbox("k");
          break;
        case 35:
          App.InjectToFocusedTextbox("l");
          break;
        case 36:
          App.InjectToFocusedTextbox("z");
          break;
        case 37:
          App.InjectToFocusedTextbox("x");
          break;
        case 38:
          App.InjectToFocusedTextbox("c");
          break;
        case 39:
          App.InjectToFocusedTextbox("v");
          break;
        case 40:
          App.InjectToFocusedTextbox("b");
          break;
        case 41:
          App.InjectToFocusedTextbox("n");
          break;
        case 42:
          App.InjectToFocusedTextbox("m");
          break;
        case 100:
          App.InputSanityCheck();
          App.HideKeyboard();
          break;
      }
    }

    public void EnterInputClick()
    {
      App.InputSanityCheck();
    }
  }
}