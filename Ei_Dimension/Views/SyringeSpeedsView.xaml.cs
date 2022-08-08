using System;
using System.Windows.Controls;

namespace Ei_Dimension.Views
{
	/// <summary>
	/// Interaction logic for SyringeSpeedsView.xaml
	/// </summary>
	public partial class SyringeSpeedsView : UserControl
	{
		public static SyringeSpeedsView Instance { get; private set; }
		public SyringeSpeedsView()
		{
			InitializeComponent();
			Instance = this;
#if DEBUG
			Console.Error.WriteLine("#21 SyringeSpeedsView Loaded");
#endif
			StartupFinalizer.Run();
		}
	}
}
