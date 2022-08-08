using System;
using System.Windows.Controls;

namespace Ei_Dimension.Views
{
	/// <summary>
	/// Interaction logic for MainButtonsView.xaml
	/// </summary>
	public partial class MainButtonsView : UserControl
	{
		public MainButtonsView()
		{
			InitializeComponent();
#if DEBUG
			Console.Error.WriteLine("#1 MainButtonsView Loaded");
#endif
		}
	}
}
