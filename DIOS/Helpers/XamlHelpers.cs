using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ei_Dimension;

public class IConvertibleConverter : IValueConverter
{
  public string ToType { get; set; }
  public string FromType { get; set; }

  #region IValueConverter Members

  object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
  {
    Type target = Type.GetType(ToType, false);
    if (target == null)
      return value;
    return Convert.ChangeType(value, target, culture);
  }

  object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
  {
    Type target = Type.GetType(FromType, false);
    if (target == null)
      return value;
    return Convert.ChangeType(value, target, culture);
  }

  #endregion
}

[ValueConversion(typeof(bool), typeof(Visibility))]
public class CollapsedVisibleConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is bool bo)
    {
      return (bo ? Visibility.Visible : Visibility.Collapsed);
    }
    throw new Exception($"Wrong usage of the {nameof(CollapsedVisibleConverter)}");
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is Visibility vis)
    {
      return vis.Equals(Visibility.Visible);
    }
    throw new Exception($"Wrong usage of the {nameof(CollapsedVisibleConverter)}");
  }
}

[ValueConversion(typeof(bool), typeof(Visibility))]
public class InverseCollapsedVisibleConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is bool bo)
    {
      return (bo ? Visibility.Collapsed : Visibility.Visible);
    }
    throw new Exception($"Wrong usage of the {nameof(CollapsedVisibleConverter)}");
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is Visibility vis)
    {
      return !vis.Equals(Visibility.Visible);
    }
    throw new Exception($"Wrong usage of the {nameof(CollapsedVisibleConverter)}");
  }
}

[ValueConversion(typeof(bool), typeof(bool))]
public class InverseBooleanConverter : IValueConverter
{
  #region IValueConverter Members

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is bool bo)
    {
      return !bo;
    }
    throw new InvalidOperationException("The target must be a boolean");
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is bool bo)
    {
      return !bo;
    }
    throw new InvalidOperationException("The target must be a boolean");
  }

  #endregion
}