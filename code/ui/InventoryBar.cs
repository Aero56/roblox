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

		if (Local.Pawn is Player player == false) return;
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
		inventoryIcon.SetClass( "hide", false);
	}

	[Event( "buildinput" )]
	public static void ProcessClientInput( InputBuilder input )
	{
		if (Local.Pawn is Player player == false) return;
		if ( player == null )
			return;

		var inventory = player.Inventory;
		if ( inventory == null )
			return;

		if ( input.Pressed( InputButton.Slot1 ) ) SelectedSlot = 0;
		if ( input.Pressed( InputButton.Slot2 ) ) SelectedSlot = 1;
		if ( input.Pressed( InputButton.Slot3 ) ) SelectedSlot = 2;
		if ( input.Pressed( InputButton.Slot4 ) ) SelectedSlot = 3;
		if ( input.Pressed( InputButton.Slot5 ) ) SelectedSlot = 4;
		if ( input.Pressed( InputButton.Slot6 ) ) SelectedSlot = 5;
		if ( input.Pressed( InputButton.Slot7 ) ) SelectedSlot = 6;
		if ( input.Pressed( InputButton.Slot8 ) ) SelectedSlot = 7;
		if ( input.Pressed( InputButton.Slot9 ) ) SelectedSlot = 8;

		if(LastSelectedSlot != SelectedSlot) 
			SetActiveSlot(input, inventory, SelectedSlot);
	}

	public static void SetActiveSlot( InputBuilder input, IBaseInventory inventory, int i )
	{
		if (Local.Pawn is Player player == false) return;

		if ( player == null )
			return;

		var ent = inventory.GetSlot( i );
		if ( ent == null )
			return;
			
		SelectedSlot = LastSelectedSlot = i;
		input.ActiveChild = ent;
	}
}
