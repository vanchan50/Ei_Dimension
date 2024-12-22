using System.Runtime.InteropServices;

namespace DIOS.Core;

[StructLayout(LayoutKind.Sequential)]
public struct BeadCompensationMatrix
{
  public float GreenB1;
  public float GreenB2;
  public float GreenB3;
  public float GreenB4;
  public float GreenB5;

  public float GreenC1;
  public float GreenC2;
  public float GreenC3;
  public float GreenC4;

  public float GreenD1;
  public float GreenD2;
  public float GreenD3;

  public float RedC1;
  public float RedC2;

  public float RedD1;
}