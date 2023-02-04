﻿namespace Grubs;

public partial class Grub
{
	public float MoveInput { get; set; }
	public float LookInput { get; set; }

	[Net, Predicted]
	public Angles LookAngles { get; set; }

	[HideInEditor]
	public Vector3 EyePosition
	{
		get => Transform.PointToWorld( EyeLocalPosition );
		set => EyeLocalPosition = Transform.PointToLocal( value );
	}

	[Net, Predicted, HideInEditor]
	public Vector3 EyeLocalPosition { get; set; }

	[HideInEditor]
	public Rotation EyeRotation
	{
		get => Transform.RotationToWorld( EyeLocalRotation );
		set => EyeLocalRotation = Transform.RotationToLocal( value );
	}

	[Net, Predicted, HideInEditor]
	public Rotation EyeLocalRotation { get; set; }

	[Net, Predicted]
	public int Facing { get; set; } = 1;

	[Net, Predicted]
	public int LastFacing { get; set; } = 1;

	public override Ray AimRay => new( EyePosition, Facing * EyeRotation.Forward );

	public void UpdateInputFromOwner( float moveInput, float lookInput )
	{
		MoveInput = moveInput;
		LookInput = -Facing * lookInput;

		var look = (LookAngles + LookInput).Normal;
		LookAngles = look
			.WithPitch( look.pitch.Clamp( -80f, 75f ) )
			.WithRoll( 0f )
			.WithYaw( 0f );

		if ( Facing != LastFacing )
			LookAngles = LookAngles.WithPitch( LookAngles.pitch * -1 );

		LastFacing = Facing;

		if ( Debug && IsTurn )
		{
			DebugOverlay.ScreenText( $"MoveInput: {MoveInput}", 13 );
			DebugOverlay.ScreenText( $"LookInput: {LookInput}", 14 );
			DebugOverlay.ScreenText( $"LookAngles: {LookAngles}", 15 );
			DebugOverlay.ScreenText( $"Facing: {Facing}", 16 );
			DebugOverlay.ScreenText( $"LastFacing: {LastFacing}", 17 );
			var tr = Trace.Ray( AimRay, 80f ).Run();
			DebugOverlay.TraceResult( tr );
		}
	}

	[ConVar.Replicated( "gr_debug_input" )]
	public static bool Debug { get; set; } = false;
}
