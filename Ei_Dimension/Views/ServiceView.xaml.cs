using System;
using System.Windows.Controls;

namespace Ei_Dimension.Views
{
	/// <summary>
	/// Interaction logic for ServiceView.xaml
	/// </summary>
	public partial class ServiceView : UserControl
	{
		public ServiceView()
		{
			InitializeComponent();
#if DEBUG
			Console.Error.WriteLine("#8 ServiceView Loaded");
#endif
		}
	}
}
