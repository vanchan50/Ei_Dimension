﻿using Newtonsoft.Json;
using DIOS.Core;
using DIOS.Application.Domain;

namespace DIOS.Application;

public class MapController
{
  public MapModel ActiveMap { get; private set; }
  public List<MapModel> MapList { get; } = new();
  public event EventHandler<MapModel> ChangedActiveMap;
  private string _mapFolder;
  private ILogger _logger;

  public MapController(string mapFolder, ILogger logger)
  {
    _mapFolder = mapFolder;

    var exePath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
    var initialMapFolderPath = $"{exePath}\\Maps";
    MoveMaps(from: initialMapFolderPath);
    UpdateMaps(exePath);
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

  public MapModel GetMapByName(string mapName)
  {
    var index = GetMapIndexByName(mapName);
    if (index == -1)
      return null;
      
    return MapList[index];
  }

  public MapModel LoadMapByName(string mapName)
  {
    MapModel map = null;
    var files = Directory.GetFiles(_mapFolder, "*.dmap");
    foreach (var mp in files)
    {
      using (TextReader reader = new StreamReader(mp))
      {
        var fileContents = reader.ReadToEnd();
        try
        {
          map = JsonConvert.DeserializeObject<MapModel>(fileContents);
          if (map.mapName != mapName)
          {
            continue;
          }
          map.Init();
          break;
        }
        catch { }
      }
    }

    var index = GetMapIndexByName(mapName);
    if (index == -1)
      return null;

    MapList[index] = map;
    ActiveMap = map;
    return MapList[index];
  }

  public MapModel GetMapByIndex(int index)
  {
    if(index < 0 || index >= MapList.Count)
      return null;
    return MapList[index];
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

  public bool SaveCalValsToCurrentMap(MapCalParameters param)
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
    if (param.TempGreenD >= 0)
      map.calcl0 = param.TempGreenD;
    if (param.TempCl1 >= 0)
      map.calcl1 = param.TempCl1;
    if (param.TempCl2 >= 0)
      map.calcl2 = param.TempCl2;
    if (param.TempCl3 >= 0)
      map.calcl3 = param.TempCl3;
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

  public bool SaveClassificationParametersToCurrentMap(string param1, string param2)
  {
    var idx = MapList.FindIndex(x => x.mapName == ActiveMap.mapName);
    var map = MapList[idx];
    if(BeadParamsHelper._shiftData.ContainsKey(param1))
      map.ClassificationParameter1 = param1;
    if (BeadParamsHelper._shiftData.ContainsKey(param2))
      map.ClassificationParameter2 = param2;

    MapList[idx] = map;
    ActiveMap = MapList[idx];

    return WriteToMap(map);
  }

  public bool SaveSensitivityChannelsToCurrentMap(string hiSensChannel, string extendedChannel)
  {
    var idx = MapList.FindIndex(x => x.mapName == ActiveMap.mapName);
    var map = MapList[idx];
    if (BeadParamsHelper._shiftData.ContainsKey(hiSensChannel))
      map.calParams.HiSensChannel = hiSensChannel;
    if (BeadParamsHelper._shiftData.ContainsKey(extendedChannel))
      map.calParams.ExtendedDNRChannel = extendedChannel;

    MapList[idx] = map;
    ActiveMap = MapList[idx];

    return WriteToMap(map);
  }

  public bool SaveReporterChannelsToCurrentMap(string reporter1, string reporter2,
    string reporter3, string reporter4, bool isSpectraplexEnabled)
  {
    var idx = MapList.FindIndex(x => x.mapName == ActiveMap.mapName);
    var map = MapList[idx];
    if (reporter1 is "None" || BeadParamsHelper._shiftData.ContainsKey(reporter1))
      map.calParams.SPReporterChannel1 = reporter1;
    if (reporter2 is "None" || BeadParamsHelper._shiftData.ContainsKey(reporter2))
      map.calParams.SPReporterChannel2 = reporter2;
    if (reporter3 is "None" || BeadParamsHelper._shiftData.ContainsKey(reporter3))
      map.calParams.SPReporterChannel3 = reporter3;
    if (reporter4 is "None" || BeadParamsHelper._shiftData.ContainsKey(reporter4))
      map.calParams.SPReporterChannel4 = reporter4;

    map.calParams.SpectraplexReporterMode = isSpectraplexEnabled;

    MapList[idx] = map;
    ActiveMap = MapList[idx];

    return WriteToMap(map);
  }


  public bool SaveCl3rChannelsToCurrentMap(string cl3rChannel, int level1,
    int level2)
  {
    var idx = MapList.FindIndex(x => x.mapName == ActiveMap.mapName);
    var map = MapList[idx];
    if (cl3rChannel is "None" || BeadParamsHelper._shiftData.ContainsKey(cl3rChannel))
      map.calParams.Cl3rChannel = cl3rChannel;
    if (level1 >= 0)
      map.calParams.Cl3rL1 = level1;
    if (level2 >= 0)
      map.calParams.Cl3rL2 = level2;

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

  public bool SaveCompensationMatrix(BeadCompensationMatrix newMatrix, bool isEnabled)
  {
    var idx = MapList.FindIndex(x => x.mapName == ActiveMap.mapName);
    var map = MapList[idx];

    map.CMatrix = newMatrix;
    map.CMatrixEnabled = isEnabled;

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

  public void MoveMaps(string from)
  {
    string[] files = null;
    try
    {
      files = Directory.GetFiles(from, "*.dmap");
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

  public void UpdateMaps(string exePath)
  {
    string path = $"{exePath}\\Maps";
    IEnumerable<string> files;
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
      var backupPath = $"{exePath}\\Backups";
      Directory.CreateDirectory(backupPath);
      var contents = JsonConvert.SerializeObject(originalMap);
      try
      {
        using (var stream =
               new StreamWriter($"{backupPath}\\{DateTime.Now.ToString("dd.MM.yyyy.hh-mm-ss", System.Globalization.CultureInfo.CreateSpecificCulture("en-GB"))}"
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