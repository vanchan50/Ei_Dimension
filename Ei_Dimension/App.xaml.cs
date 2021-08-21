using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Ei_Dimension.Models;

namespace Ei_Dimension
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    public static CustomMap CurrentMap { get; set; }
    public static MicroCy.MicroCyDevice Device { get; private set; }
    public App()
    {
      Device = new MicroCy.MicroCyDevice();
    }
  }
}
