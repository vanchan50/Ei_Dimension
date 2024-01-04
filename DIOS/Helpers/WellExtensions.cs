using DIOS.Core;
using System.Text;

namespace Ei_Dimension;

public static class WellExtensions
{
  public static string PrintActiveRegions(this Well well)
  {
    StringBuilder b = new();
    foreach (var region in well.ActiveRegions)
    {
      if (region.Number is 0)
        continue;
      b.Append($"{region.Number}, ");
    }

    if (b.Length is 0)
      b.Append("None");

    if (well.ActiveRegions.Count > 1)//accounting for region 0
    {
      var characters = 2;
      b.Remove(b.Length - characters, characters);
    }
    return $"Active Regions: {b}";
  }
}