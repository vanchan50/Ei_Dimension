using System;
using System.Windows.Controls;

namespace Ei_Dimension.Views
{
	/// <summary>
	/// Interaction logic for TemplateSelectView.xaml
	/// </summary>
	public partial class TemplateSelectView : UserControl
	{
		public static TemplateSelectView Instance { get; private set; }
		public TemplateSelectView()
		{
			InitializeComponent();
			Instance = this;
#if DEBUG
			Console.Error.WriteLine("#13 TemplateSelectView Loaded");
#endif
		}
	}
}
