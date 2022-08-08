using System.Windows.Controls;

namespace Ei_Dimension.Views
{
	/// <summary>
	/// Interaction logic for DashboardView.xaml
	/// </summary>
	public partial class DashboardView : UserControl
	{
		public static DashboardView Instance;

		public DashboardView()
		{
			InitializeComponent();
			Instance = this;
#if DEBUG
			System.Console.Error.WriteLine("#10 DashboardView Loaded");
#endif
		}
	}
}