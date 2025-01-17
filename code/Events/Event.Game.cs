namespace Grubs;

public static partial class GrubsEvent
{
	public static class Game
	{
		public const string Start = "game.start";

		/// <summary>
		/// Called when the game has started.
		/// </summary>
		public class StartAttribute : EventAttribute
		{
			public StartAttribute() : base( Start ) { }
		}

		public const string End = "game.end";

		/// <summary>
		/// Called when the game has ended.
		/// </summary>
		public class EndAttribute : EventAttribute
		{
			public EndAttribute() : base( End ) { }
		}
	}
}
