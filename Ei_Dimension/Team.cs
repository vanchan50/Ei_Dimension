using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Ei_Dimension
{

  public class Team : ObservableCollection<TeamMember>
  {

    public Team()
        : base()
    {
      Add(new TeamMember("Bob", 98.0));
      Add(new TeamMember("John", 95.0));
      Add(new TeamMember("Mike", 98.0));
      Add(new TeamMember("Julie", 95.0));
    }

    public TeamMember GetMember(string name)
    {
      TeamMember returnResult = null;

      foreach (TeamMember member in this)
      {
        if (member.Name == name)
          returnResult = member;
      }

      return returnResult;
    }

    public void GenerateNewNumbers()
    {

      Random r = new Random();

      foreach (TeamMember member in this)
      {
        member.CustomerSatScore = r.NextDouble() * 100;
      }

      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public double GetCustomerSatScore(string name)
    {
      double returnResult = double.MinValue;

      TeamMember member = GetMember(name);

      if (member != null)
        returnResult = member.CustomerSatScore;

      return returnResult;
    }

    public void SetCustomerSatScore(string name, double newCustomerSatScore)
    {
      TeamMember member = GetMember(name);

      if (member != null)
        member.CustomerSatScore = newCustomerSatScore;
    }

    public string[] TeamMembers
    {
      get
      {
        string[] returnResult = new string[this.Count];

        for (int index = 0; index < this.Count; index++)
        {
          returnResult[index] = this.Items[index].Name;
        }

        return returnResult;
      }
    }
  }
}