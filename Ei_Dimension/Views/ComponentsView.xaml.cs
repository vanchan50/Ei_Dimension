using System.Windows.Controls;

namespace Ei_Dimension.Views
{
	/// <summary>
	/// Interaction logic for ComponentsView.xaml
	/// </summary>
	public partial class ComponentsView : UserControl
	{
		public static ComponentsView Instance { get; private set; }
		public ComponentsView()
		{
			InitializeComponent();
			Instance = this;
#if DEBUG
			System.Console.Error.WriteLine("#18 ComponentsView Loaded");
#endif
		}
	}
}
