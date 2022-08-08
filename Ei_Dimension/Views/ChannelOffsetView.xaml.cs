using System.Windows.Controls;

namespace Ei_Dimension.Views
{
	/// <summary>
	/// Interaction logic for ChannelOffsetView.xaml
	/// </summary>
	public partial class ChannelOffsetView : UserControl
	{
		public static ChannelOffsetView Instance { get; private set; }
		public ChannelOffsetView()
		{
			InitializeComponent();
			Instance = this;
#if DEBUG
			System.Console.Error.WriteLine("#20 ChannelOffsetView Loaded");
#endif
		}
	}
}
