using Ei_Dimension.ViewModels;
using DIOS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ei_Dimension
{
  internal class MultiTube
  {
    private static byte _multiTubeRow;
    private static byte _multiTubeCol;
    public static void GetModifiedWellIndexes(ReadingWellEventArgs e, out byte row, out byte col)
    {
      if (WellsSelectViewModel.Instance.CurrentTableSize == 1)
      {
        row = _multiTubeRow;
        col = _multiTubeCol;  //calc for case 96 to reset position
        return;
      }
      //clear drawingboard if just switched to multitube!!! //DrawingPlate.MultitubeOverrideReset switchflip jsut for that
      row = e.Row;
      col = e.Column;
    }

    public static void Proceed()
    {
      if (WellsSelectViewModel.Instance.CurrentTableSize != 1)
        return;

      if (_multiTubeCol == 11 && _multiTubeRow == 7)
      {
        _multiTubeCol = 0;
        _multiTubeRow = 0;
        return;
      }

      if (_multiTubeCol < 11)
        _multiTubeCol++;
      else
      {
        _multiTubeCol = 0;
        _multiTubeRow++;
      }
    }
  }
}
