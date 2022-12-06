namespace Roblox;

public class InventoryBar : Panel
{
	readonly List<InventoryIcon> slots = new();
	public static int SelectedSlot { get; set; } = -1;
	public static int LastSelectedSlot { get; set; } = -1;

	public InventoryBar()
	{
		for ( int i = 0; i < 9; i++ )
		{
			var icon = new InventoryIcon( i + 1, this );
			slots.Add( icon );
		}
	}

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is Player player == false ) return;
		if ( player == null ) return;
		if ( player.Inventory == null ) return;

		for ( int i = 0; i < slots.Count; i++ )
		{
			UpdateIcon( player.Inventory.GetSlot( i ), slots[i], i );
		}
	}

	private static void UpdateIcon( Entity ent, InventoryIcon inventoryIcon, int i )
	{
		var player = Local.Pawn as Player;

		if ( ent == null )
		{
			inventoryIcon.Clear();
			return;
		}

		var di = DisplayInfo.For( ent );

		inventoryIcon.TargetEnt = ent;
		inventoryIcon.Label.Text = di.Name;
		inventoryIcon.Border.SetClass( "hide", player.ActiveChild != ent );
		inventoryIcon.SetClass( "hide", false );
	}

	[Event( "buildinput" )]
	public static void ProcessClientInput()
	{
		if ( Local.Pawn is Player player == false ) return;
		if ( player == null )
			return;

		var inventory = player.Inventory;
		if ( inventory == null )
			return;

		if ( Input.Pressed( InputButton.Slot1 ) ) SelectedSlot = 0;
		if ( Input.Pressed( InputButton.Slot2 ) ) SelectedSlot = 1;
		if ( Input.Pressed( InputButton.Slot3 ) ) SelectedSlot = 2;
		if ( Input.Pressed( InputButton.Slot4 ) ) SelectedSlot = 3;
		if ( Input.Pressed( InputButton.Slot5 ) ) SelectedSlot = 4;
		if ( Input.Pressed( InputButton.Slot6 ) ) SelectedSlot = 5;
		if ( Input.Pressed( InputButton.Slot7 ) ) SelectedSlot = 6;
		if ( Input.Pressed( InputButton.Slot8 ) ) SelectedSlot = 7;
		if ( Input.Pressed( InputButton.Slot9 ) ) SelectedSlot = 8;

		if ( LastSelectedSlot != SelectedSlot )
			SetActiveSlot( inventory, SelectedSlot );
	}

	public static void SetActiveSlot( IBaseInventory inventory, int i )
	{
		if ( Local.Pawn is not Player player )
			return;

		var ent = inventory.GetSlot( i );
		if ( player.ActiveChild == ent )
			return;

		if ( ent == null )
			return;

		player.ActiveChildInput = ent;
	}
}
