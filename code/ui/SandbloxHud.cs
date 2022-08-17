using Roblox.UI;

namespace Roblox;

[Library]
public partial class SandbloxHud : HudEntity<RootPanel>
{
	public SandbloxHud()
	{
		if ( !IsClient )
			return;

		RootPanel.StyleSheet.Load( "/ui/SandbloxHud.scss" );

		RootPanel.AddChild<Chat>();
		RootPanel.AddChild<VoiceList>();
		RootPanel.AddChild<Leaderboard<LeaderboardEntry>>();
		RootPanel.AddChild<MenuButton>();
		RootPanel.AddChild<Health>();
		RootPanel.AddChild<InventoryBar>();
	}
}
