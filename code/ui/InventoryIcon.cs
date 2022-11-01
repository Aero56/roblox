namespace Roblox;

public class InventoryIcon : Panel
{
	public Entity TargetEnt;
	public Label Label;
	public Label Number;
	public Panel Border;

	public InventoryIcon( int i, Panel parent )
	{
		Parent = parent;
		Label = Add.Label( "empty", "item-name" );
		Number = Add.Label( $"{i}", "slot-number" );
		Border = Add.Panel( "border" );
		AddEventListener( "onclick", () =>
		{
			InventoryBar.SelectedSlot = i - 1;
		} );
	}

	public void Clear()
	{
		Label.Text = "";
		SetClass( "active", false );
		SetClass( "hide", true );
	}

	public override void Tick()
	{
		base.Tick();

		SetClass( "draggingcamera", Input.Down( InputButton.SecondaryAttack ) );
	}
}
