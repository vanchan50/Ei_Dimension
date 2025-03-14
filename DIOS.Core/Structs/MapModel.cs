﻿using Newtonsoft.Json;

namespace DIOS.Core;

[JsonObject(MemberSerialization.OptOut)]
public class MapModel
{
  public string mapName;

  public int highorderidx;   //is 0 for cl0, 1 for cl1, etc
  public int midorderidx; //y
  public int loworderidx; //x
  public int calcl0;
  public int calcl1;
  public int calcl2;
  public int calcl3;
  public int calrpmaj;
  public int calrpmin;
  public int calrssc;
  public int calgssc;
  public int minVerBeads;
  public string caltime;
  public string valtime;
  public bool validation;
  public double factor;
  public CalParams calParams;

  public List<Zone> zones;
  public List<MapRegion> regions;// { get; }
  //public List<(int x, int y, int r)> classificationMap; //contains coords in 256x256 space for region numbers
  //can contain up to 6 classimaps (01,02,03,12,13,23) if necessary. possibility left for the future
  public BeadCompensationMatrix CMatrix;
  public bool CMatrixEnabled;
  public string ClassificationParameter1 = "RedC";
  public string ClassificationParameter2 = "RedD";

  [JsonIgnore]
  public SortedDictionary<int,MapRegion> Regions { get; private set; }
  [JsonIgnore]
  public bool CL0ZonesEnabled { get; private set; }

  public void Init()
  {
    Regions = new SortedDictionary<int, MapRegion>();
    foreach (var region in regions)
    {
      Regions.Add(region.Number, region);
    }

    if (zones != null && zones.Count > 0)
      CL0ZonesEnabled = true;
  }

  public float GetFactorizedNormalizationForRegion(ushort region)
  {
    return (float)(factor * Regions[region].NormalizationMFI);
  }

  public bool IsVerificationExpired(VerificationExpirationTime expiration)
  {
    bool expired = false;
    if (validation)
    {
      var valDate = DateTime.Parse(valtime, new System.Globalization.CultureInfo("en-GB"));
      switch (expiration)
      {
        case VerificationExpirationTime.Day:
          expired = valDate.AddDays(1) < DateTime.Today;
          break;
        case VerificationExpirationTime.Week:
          expired = valDate.AddDays(7) < DateTime.Today;
          break;
        case VerificationExpirationTime.Month:
          expired = valDate.AddMonths(1) < DateTime.Today;
          break;
        case VerificationExpirationTime.Quarter:
          expired = valDate.AddMonths(3) < DateTime.Today;
          break;
        case VerificationExpirationTime.Year:
          expired = valDate.AddYears(1) < DateTime.Today;
          break;
      }
    }
    return expired;
  }
}