using System.Windows.Data;

namespace Ei_Dimension.Language
{
	public class LocExtension : Binding
	{
		public LocExtension(string name)
			: base("[" + name + "]")
		{
			Mode = BindingMode.OneWay;
			Source = TranslationSource.Instance;
		}
	}
}
