namespace DIOS.Application;

public class WorkOrderController
{
  private readonly List<WorkOrder> _workOrders = new(20);
  /// <summary>
  /// Fired on creation of a new work order file
  /// </summary>
  public event EventHandler<WorkOrderEventArgs> NewWorkOrder;
  private readonly FileSystemWatcher _workOrderWatcher;
  private string _workOrderPath { get; set; }
  private string _path;

  public WorkOrderController(string workOrderFolder)
  {
    _path = workOrderFolder;
    _workOrderWatcher = new FileSystemWatcher(workOrderFolder);
    _workOrderWatcher.NotifyFilter = NotifyFilters.FileName;
    _workOrderWatcher.Filter = "*.txt";
    _workOrderWatcher.EnableRaisingEvents = true;
    _workOrderWatcher.Created += OnNewWorkOrder;
  }

  public bool TryGetWorkOrderById(string id, out WorkOrder wo)
  {
    try
    {
      wo = _workOrders.First(x => x.plateID == id);
      return true;
    }
    catch { }
    wo = new WorkOrder();
    return false;
  }

private void OnNewWorkOrder(object sender, FileSystemEventArgs e)
  {
    var name = Path.GetFileNameWithoutExtension(e.Name);
    _workOrderPath = e.FullPath;
    var order = ParseWorkOrder(e.FullPath);
    _workOrders.Add(order);
    NewWorkOrder?.Invoke(this, new WorkOrderEventArgs(order, e.Name));
  }

  public void OnAppLoaded()
  {
    string[] fileEntries = Directory.GetFiles(_path, "*.txt");
    if (fileEntries.Length == 0)
      return;

    foreach (string filePath in fileEntries)
    {
      try
      {
        var order = ParseWorkOrder(filePath);
        _workOrders.Add(order);
        var name = Path.GetFileNameWithoutExtension(filePath);
        NewWorkOrder?.Invoke(this, new WorkOrderEventArgs(order, name));
      }
      catch
      {
      }
    }
  }

  public void DoSomethingWithWorkOrder()
  {
    if (File.Exists(_workOrderPath))
      File.Delete(_workOrderPath);   //result is posted, delete work order
    //need to clear textbox in UI. this has to be an event
  }

  private WorkOrder ParseWorkOrder(string path)
  {
    using (TextReader reader = new StreamReader(path))
    {
      var contents = reader.ReadToEnd();
      return Newtonsoft.Json.JsonConvert.DeserializeObject<WorkOrder>(contents);
    }
  }
}