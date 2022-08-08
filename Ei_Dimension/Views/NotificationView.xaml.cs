using System;
using System.Windows.Controls;

namespace Ei_Dimension.Views
{
	/// <summary>
	/// Interaction logic for NotificationView.xaml
	/// </summary>
	public partial class NotificationView : UserControl
	{
		public static NotificationView Instance { get; private set; }
		public NotificationView()
		{
			InitializeComponent();
			Instance = this;
#if DEBUG
			Console.Error.WriteLine("#4 NotificationView Loaded");
#endif
		}
	}
}
