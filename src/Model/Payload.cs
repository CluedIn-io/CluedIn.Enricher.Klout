namespace CluedIn.ExternalSearch.Providers.Klout.Model
{
	public class Payload
	{
		public string kloutId { get; set; }
		public string nick { get; set; }
		public Score score { get; set; }
		public ScoreDeltas scoreDeltas { get; set; }
	}
}