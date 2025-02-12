﻿namespace Roblox;

partial class PlayerCharacter
{
	[ClientRpc]
	private void BecomeRagdollOnClient( Vector3 velocity, Vector3 forcePos, Vector3 force, int bone, bool impulse, bool blast )
	{
		ModelEntity ent = new()
		{
			Position = Position,
			Rotation = Rotation,
			Scale = Scale,
			UsePhysicsCollision = true,
			EnableAllCollisions = true,
			SurroundingBoundsMode = SurroundingBoundsType.Physics,
			RenderColor = RenderColor,
			PhysicsEnabled = true,
		};

		ent.SetModel( GetModelName() );
		ent.CopyBonesFrom( this );
		ent.CopyBodyGroups( this );
		ent.CopyMaterialGroup( this );
		ent.TakeDecalsFrom( this );
		ent.PhysicsGroup.Velocity = velocity;

		foreach ( var child in Children )
		{
			if ( !child.Tags.Has( "clothes" ) ) continue;
			if ( child is not ModelEntity e ) continue;

			var model = e.GetModelName();

			var clothing = new ModelEntity();
			clothing.SetModel( model );
			clothing.SetParent( ent, true );
			clothing.RenderColor = e.RenderColor;
			clothing.CopyBodyGroups( e );
			clothing.CopyMaterialGroup( e );
		}

		if ( impulse )
		{
			PhysicsBody body = bone > 0 ? ent.GetBonePhysicsBody( bone ) : null;

			if ( body != null )
			{
				body.ApplyImpulseAt( forcePos, force * body.Mass );
			}
			else
			{
				ent.PhysicsGroup.ApplyImpulse( force );
			}
		}

		if ( blast )
		{
			if ( ent.PhysicsGroup != null )
			{
				ent.PhysicsGroup.AddVelocity( (Position - (forcePos + Vector3.Down * 100.0f)).Normal * (force.Length * 0.2f) );
				var angularDir = (Rotation.FromYaw( 90 ) * force.WithZ( 0 ).Normal).Normal;
				ent.PhysicsGroup.AddAngularVelocity( angularDir * (force.Length * 0.02f) );
			}
		}

		Corpse = ent;

		ent.DeleteAsync( 10.0f );
	}
}
