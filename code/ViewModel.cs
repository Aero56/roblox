﻿namespace Roblox;

public class ViewModel : BaseViewModel
{
	protected static float SwingInfluence => 0.05f;
	protected static float ReturnSpeed => 5.0f;
	protected static float MaxOffsetLength => 10.0f;
	protected static float BobCycleTime => 7;
	protected static Vector3 BobDirection => new( 0.0f, 1.0f, 0.5f );

	private Vector3 swingOffset;
	private float lastPitch;
	private float lastYaw;
	private float bobAnim;

	private bool activated = false;

	public bool EnableSwingAndBob = true;

	public float YawInertia { get; private set; }
	public float PitchInertia { get; private set; }

	public override void PlaceViewmodel()
	{
		if ( !Game.LocalPawn.IsValid() )
			return;

		var inPos = Camera.Position;
		var inRot = Camera.Rotation;

		if ( !activated )
		{
			lastPitch = inRot.Pitch();
			lastYaw = inRot.Yaw();

			YawInertia = 0;
			PitchInertia = 0;

			activated = true;
		}

		var cameraBoneIndex = GetBoneIndex( "camera" );
		if ( cameraBoneIndex != -1 )
		{
			inRot *= Rotation.Inverse * GetBoneTransform( cameraBoneIndex ).Rotation;
		}

		Position = inPos;
		Rotation = inRot;

		var newPitch = Rotation.Pitch();
		var newYaw = Rotation.Yaw();

		PitchInertia = Angles.NormalizeAngle( newPitch - lastPitch );
		YawInertia = Angles.NormalizeAngle( lastYaw - newYaw );

		if ( EnableSwingAndBob )
		{
			var playerVelocity = Game.LocalPawn.Velocity;
			var verticalDelta = playerVelocity.z * Time.Delta;
			var viewDown = Rotation.FromPitch( newPitch ).Up * -1.0f;
			verticalDelta *= 1.0f - MathF.Abs( viewDown.Cross( Vector3.Down ).y );
			var pitchDelta = PitchInertia - verticalDelta * 1;
			var yawDelta = YawInertia;

			var offset = CalcSwingOffset( pitchDelta, yawDelta );
			offset += CalcBobbingOffset( playerVelocity );

			Position += Rotation * offset;
		}
		else
		{
			SetAnimParameter( "aim_yaw_inertia", YawInertia );
			SetAnimParameter( "aim_pitch_inertia", PitchInertia );
		}

		lastPitch = newPitch;
		lastYaw = newYaw;
	}

	protected Vector3 CalcSwingOffset( float pitchDelta, float yawDelta )
	{
		Vector3 swingVelocity = new( 0, yawDelta, pitchDelta );

		swingOffset -= swingOffset * ReturnSpeed * Time.Delta;
		swingOffset += swingVelocity * SwingInfluence;

		if ( swingOffset.Length > MaxOffsetLength )
		{
			swingOffset = swingOffset.Normal * MaxOffsetLength;
		}

		return swingOffset;
	}

	protected Vector3 CalcBobbingOffset( Vector3 velocity )
	{
		bobAnim += Time.Delta * BobCycleTime;

		var twoPI = MathF.PI * 2.0f;

		if ( bobAnim > twoPI )
		{
			bobAnim -= twoPI;
		}

		var speed = new Vector2( velocity.x, velocity.y ).Length;
		speed = speed > 10.0 ? speed : 0.0f;
		var offset = BobDirection * (speed * 0.005f) * System.MathF.Cos( bobAnim );
		offset = offset.WithZ( -MathF.Abs( offset.z ) );

		return offset;
	}
}
