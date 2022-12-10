namespace Roblox;

[Spawnable]
[Library( "tool_boxgun", Title = "S&bux Shooter", Description = "Shoot s&bux", Group = "fun" )]
partial class SbuxShooter : Weapon
{

	TimeSince timeSinceShoot;

	string modelToShoot = "models/robux/robux.vmdl";

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
	}

	public override async void Simulate( IClient cl )
	{
		if ( Game.IsServer )
		{
			if ( Input.Pressed( InputButton.Reload ) )
			{
				var ray = Owner.AimRay;
				var tr = Trace.Ray( ray.Position, ray.Position + ray.Forward * 4000 ).Ignore( Owner ).Run();

				if ( tr.Entity is ModelEntity ent && !string.IsNullOrEmpty( ent.GetModelName() ) )
				{
					modelToShoot = ent.GetModelName();
					Log.Trace( $"Shooting model: {modelToShoot}" );
				}
			}

			if ( Input.Pressed( InputButton.PrimaryAttack ) )
			{
				Shoot();
			}

			if ( Input.Down( InputButton.SecondaryAttack ) && timeSinceShoot > 0.05f )
			{
				timeSinceShoot = 0;
				Shoot();
			}
		}
	}

	void Shoot()
	{
		var ray = Owner.AimRay;
		
		var ent = new Prop
		{
			Position = ray.Position + ray.Forward * 50,
			Rotation = Rotation.From( new Angles( 90f, ray.Position.y, 0f ) )
		};

		ent.SetModel( modelToShoot );
		ent.Velocity = ray.Forward * new Random().Next( 500, 1000 );

		Client?.AddInt( "sbux", 1 );
	}
}
