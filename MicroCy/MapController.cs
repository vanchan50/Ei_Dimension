using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroCy
{
  public class MapController
  {
    public CustomMap ActiveMap { get; set; }
    public List<CustomMap> MapList { get; } = new List<CustomMap>();
    

    public void SaveCalVals(MapCalParameters param)
    {
      var idx = MapList.FindIndex(x => x.mapName == ActiveMap.mapName);
      var map = MapList[idx];
      if(param.TempRpMin >= 0)
        map.calrpmin = param.TempRpMin;
      if (param.TempRpMaj >= 0)
        map.calrpmaj = param.TempRpMaj;
      if (param.TempRedSsc >= 0)
        map.calrssc = param.TempRedSsc;
      if (param.TempGreenSsc >= 0)
        map.calgssc = param.TempGreenSsc;
      if (param.TempVioletSsc >= 0)
        map.calvssc = param.TempVioletSsc;
      if (param.TempCl0 >= 0)
        map.calcl0 = param.TempCl0;
      if (param.TempCl1 >= 0)
        map.calcl1 = param.TempCl1;
      if (param.TempCl2 >= 0)
        map.calcl2 = param.TempCl2;
      if (param.TempCl3 >= 0)
        map.calcl3 = param.TempCl3;
      if (param.TempFsc >= 0)
        map.calfsc = param.TempFsc;
      if (param.Compensation >= 0)
        map.calParams.compensation = param.Compensation;
      if (param.Gating >= 0)
        map.calParams.gate = (ushort)param.Gating;
      if (param.Height >= 0)
        map.calParams.height = (ushort)param.Height;
      if (param.DNRCoef >= 0)
        map.calParams.DNRCoef = param.DNRCoef;
      if (param.DNRTrans >= 0)
        map.calParams.DNRTrans = param.DNRTrans;
      if (param.MinSSC >= 0)
        map.calParams.minmapssc = (ushort)param.MinSSC;
      if (param.MaxSSC >= 0)
        map.calParams.maxmapssc = (ushort)param.MaxSSC;
      if (param.Attenuation >= 0)
        map.calParams.att = param.Attenuation;
      if (param.CL0 >= 0)
        map.calParams.CL0 = param.CL0;
      if (param.CL1 >= 0)
        map.calParams.CL1 = param.CL1;
      if (param.CL2 >= 0)
        map.calParams.CL2 = param.CL2;
      if (param.CL3 >= 0)
        map.calParams.CL3 = param.CL3;
      if (param.RP1 >= 0)
        map.calParams.RP1 = param.RP1;
      if (param.Caldate != null)
        map.caltime = param.Caldate;
      if (param.Valdate != null)
        map.valtime = param.Valdate;

      MapList[idx] = map;
      ActiveMap = MapList[idx];

      var contents = JsonConvert.SerializeObject(map);
      using (var stream = new StreamWriter(MicroCyDevice.RootDirectory.FullName + @"/Config/" + map.mapName + @".dmap"))
      {
        stream.Write(contents);
      }
    }

    public void LoadMaps()
    {
      string path = Path.Combine(MicroCyDevice.RootDirectory.FullName, "Config");
      var files = Directory.GetFiles(path, "*.dmap");
      foreach(var mp in files)
      {
        using (TextReader reader = new StreamReader(mp))
        {
          var fileContents = reader.ReadToEnd();
          try
          {
            MapList.Add(JsonConvert.DeserializeObject<CustomMap>(fileContents));
          }
          catch { }
        }
      }
    }

    public void MoveMaps()
    {
      string path = $"{Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)}\\Maps";
      string[] files = null;
      try
      {
        files = Directory.GetFiles(path, "*.dmap");
      }
      catch { return; }

      foreach (var mp in files)
      {
        string name = mp.Substring(mp.LastIndexOf("\\") + 1);
        string destination = $"{MicroCyDevice.RootDirectory.FullName}\\Config\\{name}";
        if (!File.Exists(destination))
        {
          File.Copy(mp, destination);
        }
        else
        {
          var badDate = new DateTime(2021, 12, 1);  // File.GetCreationTime(mp);
          var date = File.GetCreationTime(destination);
          date = date.Date;
          if(date < badDate)
          {
            File.Delete(destination);
            File.Copy(mp, destination);
            File.SetCreationTime(destination, DateTime.Now);
          }
        }
      }
    }
  }
}