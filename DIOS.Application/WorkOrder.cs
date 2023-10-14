using DIOS.Core;
using DIOS.Core.HardwareIntercom;

namespace DIOS.Application;

[Serializable]
public class WorkOrder
{
  public string PlateID { get; }
  public Guid BeadMapGuid { get; }
  public short WellDepth { get; }
  public DateTime CreateDateTime { get; }        //date and time per ISO8601
  public DateTime ScheduleDateTime { get; }
  public List<Well> Wells { get; }
  public uint FileSaveCheckboxes { get; }
  public PlateSize PlateSize { get; }
  public string MapName { get; }
}