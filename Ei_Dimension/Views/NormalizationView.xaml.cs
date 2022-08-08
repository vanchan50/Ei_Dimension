using System;
using System.Windows.Controls;

namespace Ei_Dimension.Views
{
	/// <summary>
	/// Interaction logic for NormalizationView.xaml
	/// </summary>
	public partial class NormalizationView : UserControl
	{
		public static NormalizationView Instance { get; private set; }
		public NormalizationView()
		{
			InitializeComponent();
			Instance = this;
#if DEBUG
			Console.Error.WriteLine("# NormalizationView Loaded");
#endif
		}
	}
}
