namespace Roblox.UI

{
	public partial class Leaderboard<T> : Panel where T : LeaderboardEntry, new()
	{
		public Panel Canvas { get; protected set; }
		readonly Dictionary<Client, T> Rows = new ();

		public Panel Header { get; protected set; }
		public static bool IsOpen { get; set; } = true;

		public Leaderboard()
		{
			StyleSheet.Load( "/ui/leaderboard/Leaderboard.scss" );
			AddClass( "leaderboard" );

			AddHeader();

			Canvas = Add.Panel( "canvas" );
		}

		public override void Tick()
		{
			base.Tick();
			
			if ( Input.Pressed( InputButton.Score ) ) IsOpen = !IsOpen;
			SetClass( "open", IsOpen );
			Canvas.SetClass( "draggingcamera", Input.Down( InputButton.SecondaryAttack ) );

			if ( !IsVisible )
				return;

			//
			// Clients that were added
			//
			foreach ( var client in Client.All.Except( Rows.Keys ) )
			{
				var entry = AddClient( client );
				Rows[client] = entry;
			}

			foreach ( var client in Rows.Keys.Except( Client.All ) )
			{
				if ( Rows.TryGetValue( client, out var row ))
				{
					row?.Delete();
					Rows.Remove( client );
				}
			}
		}


		protected virtual void AddHeader() 
		{
			Header = Add.Panel( "header" );
			Header.Add.Label( "Players", "players" );
			Header.Add.Label( "Kills", "kills" );
			Header.Add.Label( "S&bux", "sbux" );
		}

		protected virtual T AddClient( Client entry )
		{
			var p = Canvas.AddChild<T>();
			p.Client = entry;
			return p;
		}
	}
}
