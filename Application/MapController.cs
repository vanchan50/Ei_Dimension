using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using DIOS.Core;

namespace DIOS.Application
{
  public class MapController
  {
    public MapModel ActiveMap { get; private set; }
    public List<MapModel> MapList { get; } = new List<MapModel>();
    public event EventHandler<MapModel> ChangedActiveMap;
    private string _mapFolder;
    private ILogger _logger;

    public MapController(string mapFolder, ILogger logger)
    {
      _mapFolder = mapFolder;
      MoveMaps();
      UpdateMaps();
      LoadMaps();
      _logger = logger;
    }

    public void SetMap(MapModel map)
    {
      ActiveMap = map;
      OnMapChanged();
    }

    public int GetMapIndexByName(string mapName)
    {
      int i = 0;
      for (; i < MapList.Count; i++)
      {
        if (MapList[i].mapName == mapName)
          return i;
      }
      return -1;
    }

    public void OnAppLoaded(int id)
    {
      if (id > MapList.Count - 1)
      {
        try
        {
          SetMap(MapList[0]);
        }
        catch
        {
          throw new Exception($"Could not find Maps in {_mapFolder} folder");
        }
      }
      else
      {
        try
        {
          var map = MapList[id];
          SetMap(map);
        }
        catch
        {
          _logger.Log($"Problem with Maps in {_mapFolder} folder");
          SetMap(MapList[0]);
        }
      }
    }

    public bool SaveCalVals(MapCalParameters param)
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

      return WriteToMap(map);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="factor"></param>
    /// <exception cref="ArgumentException"></exception>
    public void SaveNormVals(double factor)
    {
      var idx = MapList.FindIndex(x => x.mapName == ActiveMap.mapName);
      var map = MapList[idx];
      if(factor >= 0.9 && factor <= 0.99)
        map.factor = factor;
      else
        throw new ArgumentException("Unacceptable factor value\n[0.9 - 0.99]");

      MapList[idx] = map;
      ActiveMap = MapList[idx];

      _ = WriteToMap(map);
    }

    public void SaveExtendedRangeValues(float cl1Threshold, float cl2Threshold, float cl1Multiplier, float cl2Multiplier)
    {
      var idx = MapList.FindIndex(x => x.mapName == ActiveMap.mapName);
      var map = MapList[idx];

      map.extendedRangeCL1Threshold = cl1Threshold;
      map.extendedRangeCL2Threshold = cl2Threshold;
      map.extendedRangeCL1Multiplier = cl1Multiplier;
      map.extendedRangeCL2Multiplier = cl2Multiplier;

      MapList[idx] = map;
      ActiveMap = MapList[idx];

      _ = WriteToMap(map);
    }

    public bool SaveRegions(List<MapRegion> newRegions)
    {
      if (newRegions == null || newRegions.Count == 0)
        return false;

      var idx = MapList.FindIndex(x => x.mapName == ActiveMap.mapName);
      var map = MapList[idx];

      foreach (var region in newRegions)
      {
        map.Regions[region.Number] = region;
      }

      MapList[idx] = map;
      ActiveMap = MapList[idx];
       
      return WriteToMap(map);
    }

    public void LoadMaps()  //TODO: MapLoader class that returns specific map by it's name. no need to keep all of them in memory. that's another lib between gui and core
    {
      var files = Directory.GetFiles(_mapFolder, "*.dmap");
      foreach(var mp in files)
      {
        using (TextReader reader = new StreamReader(mp))
        {
          var fileContents = reader.ReadToEnd();
          try
          {
            var map = JsonConvert.DeserializeObject<MapModel>(fileContents);
            map.Init();
            MapList.Add(map);
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
        string destination = $"{_mapFolder}\\{name}";
        if (!File.Exists(destination))
        {
          File.Copy(mp, destination);
        }
        File.Delete(mp);
      }
    }

    public void UpdateMaps()
    {
      string path = $"{Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)}\\Maps";
      string[] files = null;
      try
      {
        files = Directory.GetFiles(path, "*.dmapu");
      }
      catch { return; }

      foreach (var mp in files)
      {
        string name = mp.Substring(mp.LastIndexOf("\\") + 1);
        string destination = $"{_mapFolder}\\{name}";
        destination = destination.Substring(0, destination.Length - 1);
        if (!File.Exists(destination))
        {
          File.Copy(mp, destination);
          continue;
        }

        //load
        MapModel originalMap = null;
        MapModel updateMap = null;
        using (TextReader reader = new StreamReader(destination))
        {
          var fileContents = reader.ReadToEnd();
          try
          {
            originalMap = JsonConvert.DeserializeObject<MapModel>(fileContents);
            originalMap.Init();
          }
          catch
          {
          }
        }

        using (TextReader reader = new StreamReader(mp))
        {
          var fileContents = reader.ReadToEnd();
          try
          {
            updateMap = JsonConvert.DeserializeObject<MapModel>(fileContents);
            updateMap.Init();
          }
          catch
          {
          }
        }
        //backup
        var backupPath = $"{Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)}"
                         + "\\Backups";
          Directory.CreateDirectory(backupPath);
        var contents = JsonConvert.SerializeObject(originalMap);
        try
        {
          using (var stream =
                 new StreamWriter($"{Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)}"
                                  + $"\\Backups\\{DateTime.Now.ToString("dd.MM.yyyy.hh-mm-ss", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"))}"
                                  + originalMap.mapName + @".dmap"))
          {
            stream.Write(contents);
          }
        }
        catch
        {
          _logger.Log($"Failed to backup {originalMap.mapName}");
        }
        //swap
        foreach (var region in updateMap.Regions)
        {
          if (originalMap.Regions.ContainsKey(region.Value.Number))
          {
            originalMap.Regions[region.Value.Number] = region.Value;
            continue;
          }
          originalMap.Regions.Add(region.Key, region.Value);
        }
        //save
        if (!WriteToMap(originalMap))
          _logger.Log($"Failed to update {originalMap.mapName}");
        File.Delete(mp);
      }
    }

    private bool WriteToMap(MapModel map)
    {
      var contents = JsonConvert.SerializeObject(map);
      try
      {
        using (var stream = new StreamWriter($"{_mapFolder}\\{map.mapName}.dmap"))
        {
          stream.Write(contents);
        }
      }
      catch
      {
        return false;
      }

      return true;
    }

    private void OnMapChanged()
    {
      ChangedActiveMap?.Invoke(this, ActiveMap);
    }
  }
}