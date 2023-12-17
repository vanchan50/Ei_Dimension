using System;
using System.Windows.Controls;

namespace Ei_Dimension.Views
{
  /// <summary>
  /// Interaction logic for VerificationParametersView.xaml
  /// </summary>
  public partial class VerificationParametersView : UserControl
  {
    public static VerificationParametersView Instance { get; private set; }
    public VerificationParametersView()
    {
      InitializeComponent();
      Instance = this;
#if DEBUG
      Console.Error.WriteLine("# VerificationParametersView Loaded");
#endif
    }
  }
}
