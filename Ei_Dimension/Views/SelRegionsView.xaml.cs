using System;
using System.Windows.Controls;

namespace Ei_Dimension.Views
{
	/// <summary>
	/// Interaction logic for SelRegionsView.xaml
	/// </summary>
	public partial class SelRegionsView : UserControl
	{
		public static SelRegionsView Instance { get; private set; }
		public SelRegionsView()
		{
			InitializeComponent();
			Instance = this;
#if DEBUG
			Console.Error.WriteLine("#12 SelRegionsView Loaded");
#endif
		}
	}
}
