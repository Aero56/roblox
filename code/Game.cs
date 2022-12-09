global using Sandbox;
global using Sandbox.UI;
global using Sandbox.UI.Construct;
global using System;
global using System.Linq;
global using System.Threading.Tasks;
global using System.Collections.Generic;

using Roblox.UI;

namespace Roblox;

public partial class Game : GameManager
{
	public static new Game Current => Current as Game;

	public Game()
	{
		if ( IsServer )
		{
			// Create the HUD
			_ = new SandbloxHud();
		}
	}

	public override void ClientJoined( Client cl )
	{
		var player = new PlayerCharacter( cl );
		player.Respawn();

		Chat.AddInformation( To.Single( cl ), "Chat '/?' or '/help' for a list of chat commands." );

		cl.Pawn = player;
	}

	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		if ( cl.Pawn.IsValid() )
		{
			cl.Pawn.Delete();
			cl.Pawn = null;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	[ConCmd.Server( "spawn" )]
	public static async Task Spawn( string modelname )
	{
		var owner = ConsoleSystem.Caller?.Pawn as Player;

		if ( ConsoleSystem.Caller == null )
			return;

		var tr = Trace.Ray( owner.EyePosition, owner.EyePosition + owner.EyeRotation.Forward * 500 )
			.UseHitboxes()
			.Ignore( owner )
			.Run();

		var modelRotation = Rotation.From( new Angles( 0, owner.EyeRotation.Angles().yaw, 0 ) ) * Rotation.FromAxis( Vector3.Up, 180 );

		//
		// Does this look like a package?
		//
		if ( modelname.Count( x => x == '.' ) == 1 && !modelname.EndsWith( ".vmdl", System.StringComparison.OrdinalIgnoreCase ) && !modelname.EndsWith( ".vmdl_c", System.StringComparison.OrdinalIgnoreCase ) )
		{
			modelname = await SpawnPackageModel( modelname, owner );
			if ( modelname == null )
				return;
		}

		var model = Model.Load( modelname );
		if ( model == null || model.IsError )
			return;

		var ent = new Prop
		{
			Position = tr.EndPosition + Vector3.Down * model.PhysicsBounds.Mins.z,
			Rotation = modelRotation,
			Model = model
		};

		// Let's make sure physics are ready to go instead of waiting
		ent.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

		// If there's no physics model, create a simple OBB
		if ( !ent.PhysicsBody.IsValid() )
		{
			ent.SetupPhysicsFromOBB( PhysicsMotionType.Dynamic, ent.CollisionBounds.Mins, ent.CollisionBounds.Maxs );
		}
	}

	static async Task<string> SpawnPackageModel( string packageName, Entity source )
	{
		var package = await Package.Fetch( packageName, false );
		if ( package == null || package.PackageType != Package.Type.Model || package.Revision == null )
		{
			// spawn error particles
			return null;
		}

		if ( !source.IsValid ) return null; // source entity died or disconnected or something

		var model = package.GetMeta( "PrimaryAsset", "models/dev/error.vmdl" );

		// downloads if not downloads, mounts if not mounted
		await package.MountAsync();

		return model;
	}

	[ConCmd.Server( "spawn_entity" )]
	public static void SpawnEntity( string entName )
	{
		if ( ConsoleSystem.Caller.Pawn is Player owner == false ) return;

		if ( owner == null )
			return;

		var entityType = TypeLibrary.GetType<Entity>( entName )?.TargetType;
		if ( entityType == null )

			if ( !TypeLibrary.HasAttribute<SpawnableAttribute>( entityType ) )
				return;

		var tr = Trace.Ray( owner.EyePosition, owner.EyePosition + owner.EyeRotation.Forward * 200 )
			.UseHitboxes()
			.Ignore( owner )
			.Size( 2 )
			.Run();

		var ent = TypeLibrary.Create<Entity>( entityType );
		if ( ent is BaseCarriable && owner.Inventory != null )
		{
			if ( owner.Inventory.Add( ent, true ) )
				return;
		}

		ent.Position = tr.EndPosition;
		ent.Rotation = Rotation.From( new Angles( 0, owner.EyeRotation.Angles().yaw, 0 ) );

		//Log.Info( $"ent: {ent}" );
	}

	public override void DoPlayerNoclip( Client player )
	{
	}

	[ConCmd.Admin( "respawn_entities" )]
	public static void RespawnEntities()
	{
		Map.Reset( DefaultCleanupFilter );
	}

	public virtual async void SubmitScore( string bucket, Client client, int score )
	{

		var leaderboard = await Leaderboard.FindOrCreate( bucket, false );

		if( leaderboard != null & leaderboard.Value.CanSubmit ) {
			await leaderboard.Value.Submit( client, score );
		}
	}

	public virtual async Task<Sandbox.LeaderboardEntry?> GetScore( string name, Client client )
	{

		var leaderboard = await Leaderboard.FindOrCreate( name, false );

		return await leaderboard.Value.GetScore( client.SteamId );

	}

}
