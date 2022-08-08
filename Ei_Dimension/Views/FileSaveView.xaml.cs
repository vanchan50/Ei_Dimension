using System;
using System.Windows.Controls;

namespace Ei_Dimension.Views
{
	/// <summary>
	/// Interaction logic for FileSaveView.xaml
	/// </summary>
	public partial class FileSaveView : UserControl
	{
		public static FileSaveView Instance
		{
			get; private set;
		}
		public FileSaveView()
		{
			InitializeComponent();
			Instance = this;
#if DEBUG
			Console.Error.WriteLine("#11 FileSaveView Loaded");
#endif
		}
	}
}
