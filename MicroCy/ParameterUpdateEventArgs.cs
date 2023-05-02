using System;
using DIOS.Core.HardwareIntercom;

namespace DIOS.Core
{
  /// <summary>
  /// Parameters to update the UI information.<br/>
  /// Various types have either Float or int parameter.<br/>
  /// If the parameter is negative then use the other one
  /// </summary>
  public class ParameterUpdateEventArgs
  {
    public readonly DeviceParameterType Type;
    public readonly int Parameter;
    public readonly float FloatParameter;

    internal ParameterUpdateEventArgs(DeviceParameterType type, int intParameter = -1, float floatParameter = -1)
    {
      Type = type;
      Parameter = intParameter;
      FloatParameter = floatParameter;
    }

    public override string ToString()
    {
      return $"Type:{Enum.GetName(typeof(DeviceParameterType), Type)}\tParam:{Parameter}\tFloat:{FloatParameter}";
    }
  }
}