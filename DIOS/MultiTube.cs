using Ei_Dimension.ViewModels;
using DIOS.Core;

namespace Ei_Dimension;

internal class MultiTube
{
  private static byte _multiTubeRow;
  private static byte _multiTubeCol;
  public static void GetModifiedWellIndexes(Well well, out byte row, out byte col, bool proceed = false)
  {
    if (isATube())
    {
      row = _multiTubeRow;
      col = _multiTubeCol;  //calc for case 96 to reset position
      if (proceed)
        Proceed();
      return;
    }
    //clear drawingboard if just switched to multitube!!! //DrawingPlate._multitubeOverrideReset switchflip jsut for that
    row = well.RowIdx;
    col = well.ColIdx;
    Reset();
  }

  private static void Proceed()
  {
    if (!isATube())
      return;

    if (isLastWellFilled())
    {
      Reset();
      return;
    }

    GoToNext();
  }

  private static void GoToNext()
  {
    if (!isLastColumn())
      GoToNextColumn();
    else
    {
      GoToNextRow();
    }
  }

  private static void GoToNextColumn()
  {
    _multiTubeCol++;
  }

  private static void GoToNextRow()
  {
    _multiTubeCol = 0;
    _multiTubeRow++;
  }

  private static bool isATube()
  {
    return WellsSelectViewModel.Instance.CurrentTableSize == 1;
  }

  private static void Reset()
  {
    _multiTubeCol = 0;
    _multiTubeRow = 0;
  }

  private static bool isLastWellFilled()
  {
    return _multiTubeCol == 11 && _multiTubeRow == 7;
  }

  private static bool isLastColumn()
  {
    return _multiTubeCol >= 11;
  }
}