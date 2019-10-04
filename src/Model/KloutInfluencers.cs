using System.Collections.Generic;

namespace CluedIn.ExternalSearch.Providers.Klout.Model
{
	public class KloutInfluencers
	{
		public List<MyInfluencer> myInfluencers { get; set; }
		public List<MyInfluencee> myInfluencees { get; set; }
		public int myInfluencersCount { get; set; }
		public int myInfluenceesCount { get; set; }
	}
}