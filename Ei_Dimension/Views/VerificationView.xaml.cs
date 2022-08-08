using System;
using System.Windows.Controls;

namespace Ei_Dimension.Views
{
	/// <summary>
	/// Interaction logic for ValidationView.xaml
	/// </summary>
	public partial class VerificationView : UserControl
	{
		public static VerificationView Instance { get; private set; }
		public VerificationView()
		{
			InitializeComponent();
			Instance = this;
#if DEBUG
			Console.Error.WriteLine("#15 VerificationView Loaded");
#endif
		}
	}
}
