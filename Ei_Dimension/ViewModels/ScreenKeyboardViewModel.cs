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
					UserInputHandler.InjectToFocusedTextbox("0");
					break;
				case 1:
					UserInputHandler.InjectToFocusedTextbox("1");
					break;
				case 2:
					UserInputHandler.InjectToFocusedTextbox("2");
					break;
				case 3:
					UserInputHandler.InjectToFocusedTextbox("3");
					break;
				case 4:
					UserInputHandler.InjectToFocusedTextbox("4");
					break;
				case 5:
					UserInputHandler.InjectToFocusedTextbox("5");
					break;
				case 6:
					UserInputHandler.InjectToFocusedTextbox("6");
					break;
				case 7:
					UserInputHandler.InjectToFocusedTextbox("7");
					break;
				case 8:
					UserInputHandler.InjectToFocusedTextbox("8");
					break;
				case 9:
					UserInputHandler.InjectToFocusedTextbox("9");
					break;
				case 10:
					UserInputHandler.InjectToFocusedTextbox("");
					break;
				case 11:
					UserInputHandler.InjectToFocusedTextbox(".");
					break;
				case 12:
					UserInputHandler.InjectToFocusedTextbox("-");
					break;
				case 13:
					UserInputHandler.InjectToFocusedTextbox("+");
					break;
				case 14:
					UserInputHandler.InjectToFocusedTextbox(" ");
					break;
				case 15:
					UserInputHandler.InjectToFocusedTextbox("(");
					break;
				case 16:
					UserInputHandler.InjectToFocusedTextbox(")");
					break;
				case 17:
					UserInputHandler.InjectToFocusedTextbox("q");
					break;
				case 18:
					UserInputHandler.InjectToFocusedTextbox("w");
					break;
				case 19:
					UserInputHandler.InjectToFocusedTextbox("e");
					break;
				case 20:
					UserInputHandler.InjectToFocusedTextbox("r");
					break;
				case 21:
					UserInputHandler.InjectToFocusedTextbox("t");
					break;
				case 22:
					UserInputHandler.InjectToFocusedTextbox("y");
					break;
				case 23:
					UserInputHandler.InjectToFocusedTextbox("u");
					break;
				case 24:
					UserInputHandler.InjectToFocusedTextbox("i");
					break;
				case 25:
					UserInputHandler.InjectToFocusedTextbox("o");
					break;
				case 26:
					UserInputHandler.InjectToFocusedTextbox("p");
					break;
				case 27:
					UserInputHandler.InjectToFocusedTextbox("a");
					break;
				case 28:
					UserInputHandler.InjectToFocusedTextbox("s");
					break;
				case 29:
					UserInputHandler.InjectToFocusedTextbox("d");
					break;
				case 30:
					UserInputHandler.InjectToFocusedTextbox("f");
					break;
				case 31:
					UserInputHandler.InjectToFocusedTextbox("g");
					break;
				case 32:
					UserInputHandler.InjectToFocusedTextbox("h");
					break;
				case 33:
					UserInputHandler.InjectToFocusedTextbox("j");
					break;
				case 34:
					UserInputHandler.InjectToFocusedTextbox("k");
					break;
				case 35:
					UserInputHandler.InjectToFocusedTextbox("l");
					break;
				case 36:
					UserInputHandler.InjectToFocusedTextbox("z");
					break;
				case 37:
					UserInputHandler.InjectToFocusedTextbox("x");
					break;
				case 38:
					UserInputHandler.InjectToFocusedTextbox("c");
					break;
				case 39:
					UserInputHandler.InjectToFocusedTextbox("v");
					break;
				case 40:
					UserInputHandler.InjectToFocusedTextbox("b");
					break;
				case 41:
					UserInputHandler.InjectToFocusedTextbox("n");
					break;
				case 42:
					UserInputHandler.InjectToFocusedTextbox("m");
					break;
				case 100:
					UserInputHandler.InputSanityCheck();
					App.HideKeyboard();
					break;
			}
		}

		public void EnterInputClick()
		{
			UserInputHandler.InputSanityCheck();
		}
	}
}