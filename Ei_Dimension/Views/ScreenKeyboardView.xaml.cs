using System;
using System.Windows.Controls;

namespace Ei_Dimension.Views
{
	/// <summary>
	/// Interaction logic for ScreenKeyboardView.xaml
	/// </summary>
	public partial class ScreenKeyboardView : UserControl
	{
		public ScreenKeyboardView()
		{
			InitializeComponent();
#if DEBUG
			Console.Error.WriteLine("#3 ScreenKeyboardView Loaded");
#endif
		}
	}
}
