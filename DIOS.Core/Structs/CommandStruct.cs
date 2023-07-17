namespace DIOS.Core
{
  public struct CommandStruct
  {
    public byte Code;
    public byte Command;
    public ushort Parameter;
    public float FParameter;

    public override string ToString()
    {
      return string.Format("{0:X2},{1:X2},{2},{3:F3}", Code, Command, Parameter, FParameter);
    }
  }
}
