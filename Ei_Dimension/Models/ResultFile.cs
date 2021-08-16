using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ei_Dimension.Core;
using System.IO;

namespace Ei_Dimension.Models
{
  public class ResultFile
  {
    public string Name { get; set; }
    public string Path { get; set; }
    public DateTime Created { get; set; }
    public ResultFile(string fname)
    {
      Path = fname;
      Name = Path.Substring(Path.LastIndexOf('\\') + 1);
      Created = File.GetCreationTime(Path);
    }
  }
}
