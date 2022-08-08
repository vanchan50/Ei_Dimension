using System.Windows.Controls;

namespace Ei_Dimension.Views
{
	/// <summary>
	/// Interaction logic for PlatePictogramView.xaml
	/// </summary>
	public partial class PlatePictogramView : UserControl
	{
		public static PlatePictogramView Instance { get; private set; }
		public PlatePictogramView()
		{
			InitializeComponent();
			Instance = this;
		}
	}
}
