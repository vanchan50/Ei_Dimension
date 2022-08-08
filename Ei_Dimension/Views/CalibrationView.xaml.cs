using System.Windows.Controls;

namespace Ei_Dimension.Views
{
	/// <summary>
	/// Interaction logic for CalibrationView.xaml
	/// </summary>
	public partial class CalibrationView : UserControl
	{
		public static CalibrationView Instance { get; private set; }
		public CalibrationView()
		{
			InitializeComponent();
			Instance = this;
#if DEBUG
			System.Console.Error.WriteLine("#14 CalibrationView Loaded");
#endif
		}
	}
}
