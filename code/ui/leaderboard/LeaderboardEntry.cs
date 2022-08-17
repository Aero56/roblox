namespace Roblox.UI

{
	public partial class LeaderboardEntry : Panel
	{
		public Client Client;

		public Label PlayerName;
		public Label Kills;
		public Label Sbux;

		public LeaderboardEntry()
		{
			AddClass( "entry" );

			PlayerName = Add.Label( "PlayerName", "name" );
			Kills = Add.Label( "", "kills" );
			Sbux = Add.Label( "", "sbux" );
		}

		RealTimeSince TimeSinceUpdate = 0;

		public override void Tick()
		{
			base.Tick();

			if ( !IsVisible )
				return;

			if ( !Client.IsValid() )
				return;

			if ( TimeSinceUpdate < 0.1f )
				return;

			TimeSinceUpdate = 0;
			UpdateData();
		}

		public virtual void UpdateData()
		{
			PlayerName.Text = Client.Name;
			Kills.Text = Client.GetInt( "kills" ).ToString();
			Sbux.Text = Client.GetInt( "sbux" ).ToString();
			SetClass( "me", Client == Local.Client );
		}

		public virtual void UpdateFrom( Client client )
		{
			Client = client;
			UpdateData();
		}
	}
}
