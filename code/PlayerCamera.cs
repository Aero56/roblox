namespace Roblox;

public partial class PlayerCamera
{

	public Vector3 Position { get; set; }
	public Rotation Rotation { get; set; }
	public float FieldOfView { get; set; }

	protected Angles OrbitAngles;

	public static float OrbitDistance { get; set; } = 400f;
	protected float TargetOrbitDistance { get; set; } = 400f;
	protected static float WheelSpeed => 100f;

	protected Range CameraDistance { get; set; } = new( 0, 1800 );
	protected Range PitchClamp { get; set; } = new( -89, 89 );
	public static float ZFarPreference { get; set; } = 80000f;

	public AnimatedEntity TargetEntity { get; set; }

	public static bool IsFirstPerson { get; set; } = false;

	protected static Rotation LookAt( Vector3 targetPosition, Vector3 position )
	{
		var targetDelta = targetPosition - position;
		var direction = targetDelta.Normal;

		return Rotation.From( new Angles(
			((float)Math.Asin( direction.z )).RadianToDegree() * -1.0f,
			((float)Math.Atan2( direction.y, direction.x )).RadianToDegree(),
			0.0f ) );
	}

	protected void UpdateWithoutTarget()
	{
		var target = Entity.All.FirstOrDefault( x => x is SpawnPoint );

		if ( target.IsValid() )
		{
			Position = target.Position + Vector3.Up * 300f + Vector3.Forward * 300f;
			Rotation = LookAt( target.Position, Position );
		}
		else
		{
			// At this point... who cares, really
		}
	}

	public void Update()
	{
		var pawn = TargetEntity;
		if ( !pawn.IsValid() )
			TargetEntity = FindTargetEntity();

		if ( !pawn.IsValid() )
		{
			UpdateWithoutTarget();
			return;
		}

		Position = pawn.Position;
		Vector3 targetPos;


		Position += Vector3.Up * (pawn.CollisionBounds.Center.z * pawn.Scale);
		Rotation = Rotation.From( OrbitAngles );

		targetPos = Position + Rotation.Backward * OrbitDistance;

		var tr = Trace.Ray( Position, targetPos )
			.WithAnyTags( "solid" )
			.Ignore( pawn )
			.Radius( 8 )
			.Run();

		Position = tr.EndPosition;

		FieldOfView = 70f;

		Camera.Position = Position;
		Camera.Rotation = Rotation;
		Camera.ZFar = ZFarPreference;
		Camera.FirstPersonViewer = null;
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( FieldOfView );
	}

	protected static PlayerCharacter Player => Local.Pawn as PlayerCharacter;

	private static AnimatedEntity FindTargetEntity()
	{
		var localPawn = Local.Pawn;

		if ( localPawn is Player character )
		{
			return character;
		}
		else
		{
			var target = Client.All.Select( x => x.Pawn as Player )
				.FirstOrDefault();

			if ( target.IsValid() )
			{
				return target;
			}
		}

		return null;
	}

	public bool IsSpectator => Local.Pawn != TargetEntity;

	public void BuildInput()
	{
		var pawn = TargetEntity;

		var wheel = Input.MouseWheel;
		if ( wheel != 0 )
		{
			TargetOrbitDistance -= wheel * WheelSpeed;
			TargetOrbitDistance = TargetOrbitDistance.Clamp( CameraDistance.Min, CameraDistance.Max );
		}

		OrbitDistance = OrbitDistance.LerpTo( TargetOrbitDistance, Time.Delta * 10f );

		if ( Input.UsingController )
		{
			OrbitAngles.yaw += Input.AnalogLook.yaw;
			OrbitAngles.pitch += Input.AnalogLook.pitch;
			OrbitAngles = OrbitAngles.Normal;

			if ( !IsSpectator && Player.IsValid() )
				Player.ViewAngles = OrbitAngles.WithPitch( 0f );
		}
		else if ( Input.Down( InputButton.SecondaryAttack ) )
		{
			OrbitAngles.yaw += Input.AnalogLook.yaw;
			OrbitAngles.pitch += Input.AnalogLook.pitch;
			OrbitAngles = OrbitAngles.Normal;
		}

		if ( !IsSpectator && Player.IsValid() && (Input.Down( InputButton.Forward ) || Input.Down( InputButton.Back ) || Input.Down( InputButton.Left ) || Input.Down( InputButton.Right )) )
		{
			Player.ViewAngles = OrbitAngles.WithPitch( 0f ) + Input.AnalogMove.EulerAngles.WithPitch( 0f );
		}

		OrbitAngles.pitch = OrbitAngles.pitch.Clamp( PitchClamp.Min, PitchClamp.Max );

		// Let players move around at will
		if ( !IsSpectator && Player.IsValid() )
			Player.InputDirection = Rotation.From( OrbitAngles.WithPitch( 0f ) ) * Input.AnalogMove;

		Sound.Listener = new()
		{
			Position = pawn.IsValid() ? pawn.AimRay.Position : Position,
			Rotation = Rotation
		};
	}
}
