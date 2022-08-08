using System.Windows.Controls;

namespace Ei_Dimension.Views
{
	/// <summary>
	/// Interaction logic for ChannelsView.xaml
	/// </summary>
	public partial class ChannelsView : UserControl
	{
		public static ChannelsView Instance { get; private set; }
		public ChannelsView()
		{
			InitializeComponent();
			Instance = this;
#if DEBUG
			System.Console.Error.WriteLine("#16 ChannelsView Loaded");
#endif
		}
	}
}
