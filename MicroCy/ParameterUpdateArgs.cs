namespace DIOS.Core
{
  /// <summary>
  /// Parameters to update the UI information.<br/>
  /// Various types have either Float or int parameter.<br/>
  /// If the parameter is negative then use the other one
  /// </summary>
  public readonly struct ParameterUpdateArgs
  {
    public readonly DeviceParameterType Type;
    public readonly int Parameter;
    public readonly float FloatParameter;

    internal ParameterUpdateArgs(DeviceParameterType type, int parameter = -1, float fParameter = -1)
    {
      Type = type;
      Parameter = parameter;
      FloatParameter = fParameter;
    }
  }
}
