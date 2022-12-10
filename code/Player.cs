namespace Roblox;

public partial class PlayerCharacter : Player
{

	private DamageInfo lastDamage;

	/// <summary>
	/// The clothing container is what dresses the citizen
	/// </summary>
	public ClothingContainer Clothing = new();

	[ClientInput] public new Vector3 InputDirection { get; set; }
	[ClientInput] public new Angles ViewAngles { get; set; }
	[ClientInput] public Vector3 CursorDirection { get; set; }
	[ClientInput] public Vector3 CursorOrigin { get; set; }

	PlayerCamera PlayerCamera = new PlayerCamera();

	/// <summary>
	/// Default init
	/// </summary>
	public PlayerCharacter()
	{
		Inventory = new Inventory( this );
	}

	/// <summary>
	/// Initialize using this client
	/// </summary>
	public PlayerCharacter( IClient cl ) : this()
	{
		// Load clothing from client data
		Clothing.LoadFromClient( cl );
	}

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new CharacterController();

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		Clothing.DressEntity( this );

		Inventory.Add( new Fists() );
		Inventory.Add( new Flashlight() );
		Inventory.Add( new Pistol() );
		Inventory.Add( new SbuxShooter() );

		base.Respawn();
	}

	public override void BuildInput()
	{

		if ( Input.StopProcessing ) return;

		ActiveChild?.BuildInput();

		if ( Input.StopProcessing ) return;

		Controller?.BuildInput();

		if ( Input.StopProcessing ) return;
		
		PlayerCamera?.BuildInput();

	}

	public override void OnKilled()
	{
		base.OnKilled();

		PlaySound( "oof" );

		BecomeRagdollOnClient( Velocity, lastDamage.Position, lastDamage.Force, lastDamage.BoneIndex, lastDamage.HasTag( "bullet" ), lastDamage.HasTag( "blast" ) );

		Controller = null;

		EnableAllCollisions = false;
		EnableDrawing = false;

		foreach ( var child in Children )
		{
			child.EnableDrawing = false;
		}

		Inventory.DropActive();
		Inventory.DeleteContents();
	}

	public override async void TakeDamage( DamageInfo info )
	{
		if ( info.Hitbox.HasTag( "head" ) )
		{
			info.Damage *= 10.0f;
		}

		lastDamage = info;

		base.TakeDamage( info );
	}

	public override PawnController GetActiveController()
	{
		if ( DevController != null ) return DevController;

		return base.GetActiveController();
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		PlayerCamera.Update();
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if ( LifeState != LifeState.Alive )
			return;

		var controller = GetActiveController();
		if ( controller != null )
		{
			SimulateAnimation( controller );
		}

		TickPlayerUse();
		SimulateActiveChild( cl, ActiveChild );
	}

	Entity lastWeapon;

	void SimulateAnimation( PawnController controller )
	{
		if ( controller == null )
			return;

		// where should we be rotated to
		var turnSpeed = 0.02f;

		Rotation rotation;

		// If we're a bot, spin us around 180 degrees.
		if ( Client.IsBot )
			rotation = ViewAngles.WithYaw( ViewAngles.yaw + 180f ).ToRotation();
		else
			rotation = ViewAngles.ToRotation();

		var idealRotation = Rotation.LookAt( rotation.Forward.WithZ( 0 ), Vector3.Up );

		Rotation = Rotation.Slerp( Rotation, idealRotation, controller.WishVelocity.Length * Time.Delta * turnSpeed );
		Rotation = Rotation.Clamp( idealRotation, 90.0f, out var shuffle ); // lock facing to within 45 degrees of look direction

		var animHelper = new CitizenAnimationHelper( this );

		animHelper.WithWishVelocity( controller.WishVelocity );
		animHelper.WithVelocity( controller.Velocity );
		animHelper.WithLookAt( EyePosition + EyeRotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
		animHelper.AimAngle = rotation;
		animHelper.FootShuffle = shuffle;
		animHelper.IsGrounded = GroundEntity != null;
		animHelper.IsSitting = controller.HasTag( "sitting" );
		animHelper.IsClimbing = controller.HasTag( "climbing" );
		animHelper.IsSwimming = this.GetWaterLevel() >= 0.5f;
		animHelper.IsWeaponLowered = false;

		if ( controller.HasEvent( "jump" ) ) animHelper.TriggerJump();
		if ( ActiveChild != lastWeapon ) animHelper.TriggerDeploy();


		if ( ActiveChild is BaseCarriable carry )
		{
			carry.SimulateAnimator( animHelper );
		}
		else
		{
			animHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
			animHelper.AimBodyWeight = 0.5f;
		}

		lastWeapon = ActiveChild;
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );
	}

	[ConCmd.Server( "inventory_current" )]
	public static void SetInventoryCurrent( string entName )
	{
		if ( ConsoleSystem.Caller.Pawn is Player target == false ) return;

		var inventory = target.Inventory;
		if ( inventory == null )
			return;

		for ( int i = 0; i < inventory.Count(); ++i )
		{
			var slot = inventory.GetSlot( i );
			if ( !slot.IsValid() )
				continue;

			if ( slot.ClassName != entName )
				continue;

			inventory.SetActiveSlot( i, false );

			break;
		}
	}
}
