using System.Windows.Controls;

namespace Ei_Dimension.Views
{
	/// <summary>
	/// Interaction logic for AlignmentView.xaml
	/// </summary>
	public partial class AlignmentView : UserControl
	{
		public static AlignmentView Instance { get; private set; }
		public AlignmentView()
		{
			InitializeComponent();
			Instance = this;
#if DEBUG
			System.Console.Error.WriteLine("#19 AlignmentView Loaded");
#endif
		}
	}
}
