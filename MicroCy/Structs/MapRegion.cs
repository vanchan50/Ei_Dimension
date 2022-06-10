using System;
using System.Collections.Generic;

namespace DIOS.Core
{
  [Serializable]
  public class MapRegion
  {
    public int Number;
    public double VerificationTargetReporter;
    public int NormalizationMFI;
    public bool isValidator;
    public (int x, int y) Center; //coords in 256x256 space
    public List<(int x, int y)> Points; //contains coords in 256x256 space for region numbers

    /// <summary>
    /// Find the nearest region from the collection
    /// </summary>
    /// <param name="regionsToCompare">A collection of regions to get the result from</param>
    /// <returns></returns>
    public MapRegion FindNearestRegionFrom(ICollection<MapRegion> regionsToCompare)
    {
      if (regionsToCompare.Count < 1)
        return null;
      double distanceSquared = double.MaxValue;
      MapRegion nearest = null;
      foreach (var region in regionsToCompare)
      {
        var distSq = Math.Pow(region.Center.x - Center.x, 2) + Math.Pow(region.Center.y - Center.y, 2);
        if (distanceSquared < distSq)
          nearest = region;
      }
      return nearest;
    }
  }
}