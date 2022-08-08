using System;
using System.Windows.Controls;

namespace Ei_Dimension.Views
{
	/// <summary>
	/// Interaction logic for MaintenanceView.xaml
	/// </summary>
	public partial class MaintenanceView : UserControl
	{
		public static MaintenanceView Instance { get; private set; }
		public MaintenanceView()
		{
			InitializeComponent();
			Instance = this;
#if DEBUG
			Console.Error.WriteLine("#7 MaintenanceView Loaded");
#endif
		}
	}
}