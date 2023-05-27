namespace DIOS.Application
{
  public enum Termination
  {
    ///<summary>Triggers when every defined region has a set amount of beads</summary>
    MinPerRegion = 0,
    ///<summary>Triggers when a set amount of beads is captured for a well</summary>
    TotalBeadsCaptured = 1,
    ///<summary>default case for the instrument, Triggers when one of the syringes is emptied</summary>
    EndOfSample = 2,
    ///<summary>Triggers when a set amount of time has passed for a well</summary>
    Timer = 3
  }
}
