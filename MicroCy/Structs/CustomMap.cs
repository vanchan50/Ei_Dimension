using System.Collections.Generic;
using System;
using Newtonsoft.Json;

namespace DIOS.Core
{
  [JsonObject(MemberSerialization.Fields)]
  public class CustomMap
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
    public int calvssc;
    public int calfsc;
    public string caltime;
    public string valtime;
    public bool validation;
    public double factor;
    public CalParams calParams;

    public List<MapRegion> regions;// { get; }
    //public List<(int x, int y, int r)> classificationMap; //contains coords in 256x256 space for region numbers
    //can contain up to 6 classimaps (01,02,03,12,13,23) if necessary. possibility left for the future
    [JsonIgnore]
    public SortedDictionary<int,MapRegion> Regions { get; private set; }

    public void Init()
    {
      Regions = new SortedDictionary<int, MapRegion>();
      foreach (var region in regions)
      {
        Regions.Add(region.Number, region);
      }
    }

    public float GetFactorizedNormalizationForRegion(int region)
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
}