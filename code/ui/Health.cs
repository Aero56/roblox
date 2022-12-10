namespace Roblox;

public class Health : Panel
{
	public Panel HealthBar;
	protected float LerpedHealthFraction { get; set; } = 1f;

	public Health()
	{
		HealthBar = Add.Panel( "healthbar" );
	}

	public override void Tick()
	{
		var player = Game.LocalPawn;
		if ( player == null ) return;

		LerpedHealthFraction = LerpedHealthFraction.LerpTo( player.Health / 100, Time.Delta * 10f );
		HealthBar.Style.Width = Length.Fraction( LerpedHealthFraction );

		// SetClass("hide", player.Health.CeilToInt() == 100);

	}
}
