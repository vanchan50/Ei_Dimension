using DIOS.Core;

namespace DIOS.Application;

[Serializable]
public class WorkOrder
{
  public string PlateID { get; }
  public Guid BeadMapGuid { get; }
  public short NumberRows { get; }
  public short NumberCols { get; }
  public short WellDepth { get; }
  public DateTime createDateTime { get; }        //date and time per ISO8601
  public DateTime scheduleDateTime { get; }
  public List<Well> Wells { get; }
}