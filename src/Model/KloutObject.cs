using System.Collections.Generic;

namespace CluedIn.ExternalSearch.Providers.Klout.Model
{
	public class KloutObject
	{
		public KloutInfluencers Influencers { get; set; }
		public IList<KloutTopic> Topics { get; set; }
		public KloutUser User { get; set; }
		public string Twitter { get; set; }
	}
}