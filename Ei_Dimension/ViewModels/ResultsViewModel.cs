using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using System;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using DevExpress.Xpf.Charts;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ResultsViewModel
  {
    public virtual SampleData1 SampleX {get;set;}
    protected ResultsViewModel()
    {
      SampleX = new SampleData1();
    }

    public static ResultsViewModel Create()
    {
      return ViewModelSource.Create(() => new ResultsViewModel());
    }

    public void AddItem()
    {
      var r = new Random();
      
      SampleX.Add(new Scatter(r.Next(400,1000),r.Next(1,100)));
    }

  }
}