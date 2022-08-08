using System.Windows;
using System.Windows.Controls;

namespace Ei_Dimension.Views
{
	/// <summary>
	/// Interaction logic for ExperimentView.xaml
	/// </summary>
	public partial class ExperimentView : UserControl
	{
		public static ExperimentView Instance { get; private set; }
		public ExperimentView()
		{
			InitializeComponent();
			Instance = this;
#if DEBUG
			System.Console.Error.WriteLine("#5 ExperimentView Loaded");
#endif
		}

		private void DbTube_Click(object sender, RoutedEventArgs e)
		{
			DbButton.IsChecked = true;
		}
	}
}
