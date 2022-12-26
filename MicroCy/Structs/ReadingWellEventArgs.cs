namespace DIOS.Core
{
  public class ReadingWellEventArgs
  {
    public Well Well { get;}
    public ReadingWellEventArgs(Well well)
    {
      Well = well;
    }
  }
}
