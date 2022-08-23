using Roblox.UI;

namespace Roblox;

public class MenuButton : Panel
{
    public Panel Circle;
    public Panel Dots;
	public MenuButton()
	{
        Circle = Add.Panel( "circle" );
        Circle.Add.Panel("dot");
        Circle.Add.Panel("dot");
        Circle.Add.Panel("dot");
        // cba making an svg
        AddEventListener("onclick", () => Leaderboard<LeaderboardEntry>.IsOpen = !Leaderboard<LeaderboardEntry>.IsOpen);
	}

    public override void Tick()
	{
		base.Tick();

		SetClass( "draggingcamera", Input.Down( InputButton.SecondaryAttack ) );
	}
}
