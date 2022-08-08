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
				case 100:
					App.HideNumpad();
					break;
			}
		}

		public void EnterInputClick()
		{
			App.HideNumpad();
		}
	}
}