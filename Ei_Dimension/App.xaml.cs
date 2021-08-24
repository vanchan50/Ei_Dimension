using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Ei_Dimension
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    public static MicroCy.MicroCyDevice Device { get; private set; }
    public App()
    {
      SetLanguage("en-US");

      Device = new MicroCy.MicroCyDevice();
      Device.LoadMaps();
      Device.activemap = Device.maplist[0];
      //reading VM add SetControl(m_MicroCy.systemControl);
      Device.systemControl = Settings.Default.systemControl;
      Device.Outdir = Settings.Default.DirPath;
      Device.subtregbg = Settings.Default.subtregbg;
      Device.LISInputdir = Settings.Default.LISdirPath;
      // reading VM add .SelectedIndex = Properties.Settings.Default.defaultMap;
      // calibration VM add Properties.Settings.Default.compensation.ToString();
      // reading VM add slist.DataSource = active_items;
      Device.sampleSize = Settings.Default.sampleSize;
      // RegCtr_SampSize.Text = Device.sampleSize.ToString();
      Device.everyevent = Settings.Default.everyevent;
      Device.rmeans = Settings.Default.rmeans;
      // rmeanscb.Checked = m_MicroCy.rmeans;
      Device.pltrept = Settings.Default.pltrept;
      // plateResultscb.Checked = m_MicroCy.pltrept;
      // m_MicroCy.InitSTab("readertab");

    }

    public static void SetLanguage(string locale)
    {
      if (string.IsNullOrEmpty(locale))
        locale = "en-US";
      Language.TranslationSource.Instance.CurrentCulture = new System.Globalization.CultureInfo(locale);
    }
  }
}
