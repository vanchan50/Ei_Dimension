using System.Windows;
using System.Windows.Media;

namespace Ei_Dimension.Styles
{
	public class EmbeddedImg
	{
		public static readonly DependencyProperty ImageProperty;

		static EmbeddedImg()
		{
			//register attached dependency property
			var metadata = new FrameworkPropertyMetadata((ImageSource)null);
			ImageProperty = DependencyProperty.RegisterAttached("Image",
			  typeof(ImageSource), typeof(EmbeddedImg), metadata);
		}

		public static ImageSource GetImage(DependencyObject obj)
		{
			return (ImageSource)obj.GetValue(ImageProperty);
		}

		public static void SetImage(DependencyObject obj, ImageSource value)
		{
			obj.SetValue(ImageProperty, value);
		}
	}
}
