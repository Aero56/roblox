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

	public override void Simulate( Client cl )
	{
		if ( Host.IsServer )
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

			if ( Input.Released( InputButton.PrimaryAttack ) || Input.Released( InputButton.SecondaryAttack ) )
			{
				UpdateLeaderboard( cl );
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

	public static void UpdateLeaderboard( Client cl )
	{
		var sbuxCount = cl.GetInt( "sbux" );
		Game.Current.SubmitScore( "Sandbux", cl, sbuxCount );

	}
}
