﻿using DIOS.Core;

namespace DIOS.Application;

[Serializable]
public struct WorkOrder
{
  public string plateID;
  public Guid beadMapId;
  public short numberRows;
  public short numberCols;
  public short wellDepth;
  public DateTime createDateTime;        //date and time per ISO8601
  public DateTime scheduleDateTime;
  public List<Well> woWells;
}