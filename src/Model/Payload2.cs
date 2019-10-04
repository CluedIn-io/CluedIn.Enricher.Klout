namespace CluedIn.ExternalSearch.Providers.Klout.Model
{
	public class Payload2
	{
		public string kloutId { get; set; }
		public string nick { get; set; }
		public Score2 score { get; set; }
		public ScoreDeltas2 scoreDeltas { get; set; }
	}
}