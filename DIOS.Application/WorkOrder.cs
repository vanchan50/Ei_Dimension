using DIOS.Core;

namespace DIOS.Application;

[Serializable]
public class WorkOrder
{
  public string PlateID { get; }
  public Guid BeadMapGuid { get; }
  public short WellDepth { get; }
  public DateTime CreateDateTime { get; }        //date and time per ISO8601
  public DateTime ScheduleDateTime { get; }
  public string WellReadingSpeed { get; } //turn into enum hispeed/lowspeed/etc
  public Termination TerminationType { get; }
  public List<Well> Wells { get; }
}