using System;
using System.Windows.Controls;

namespace Ei_Dimension.Views
{
	/// <summary>
	/// Interaction logic for NumpadView.xaml
	/// </summary>
	public partial class NumpadView : UserControl
	{
		public NumpadView()
		{
			InitializeComponent();
#if DEBUG
			Console.Error.WriteLine("#2 NumpadView Loaded");
#endif
		}
	}
}
