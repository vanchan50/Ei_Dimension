namespace DIOS.Application;

public class WorkOrderEventArgs
{
  public string FileName { get; }
  public WorkOrder WorkOrder { get; }

  public WorkOrderEventArgs(WorkOrder wo, string fileName)
  {
    WorkOrder = wo;
    FileName = fileName;
  }
}