using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ei_Dimension
{
  public class SampleData1 : ObservableCollection<Scatter>
  {
    public SampleData1()
    {
      Add(new Scatter(500, 30));
      Add(new Scatter(600, 60));
      Add(new Scatter(700, 40));
    }

  }
}
