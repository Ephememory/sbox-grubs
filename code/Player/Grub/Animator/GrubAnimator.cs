﻿namespace Grubs;

public class GrubAnimator : EntityComponent<Grub>
{
	private float _incline;

	private bool _backflipping;
	private TimeSince _timeSinceBackflip;

	public void Backflip()
	{
		_backflipping = true;
		_timeSinceBackflip = 0f;
	}

	public virtual void Simulate( IClient client )
	{
		var grub = Entity;
		var ctrl = grub.Controller;

		if ( ctrl is null )
			return;

		grub.SetAnimParameter( "backflip", _backflipping );
		grub.SetAnimParameter( "grounded", ctrl.IsGrounded );
		grub.SetAnimParameter( "aimangle", grub.EyeRotation.Pitch() * -grub.Facing );
		grub.SetAnimParameter( "velocity", ctrl.GetWishVelocity().Length );
		grub.SetAnimParameter( "lowhp", grub.Health <= 20f );
		grub.SetAnimParameter( "explode", grub.LifeState == LifeState.Dying );

		if ( _backflipping && _timeSinceBackflip > Time.Delta * 2f )
		{
			_backflipping = false;
		}

		var airMove = ctrl.GetMechanic<AirMoveMechanic>();
		if ( airMove is not null )
			grub.SetAnimParameter( "hardfall", airMove.IsHardFalling );

		var tr = Trace.Ray( grub.Position + grub.Rotation.Up * 10f, grub.Position + grub.Rotation.Down * 128 )
			.Size( 2f )
			.Ignore( grub )
			.IncludeClientside()
			.Run();
		_incline = MathX.Lerp( _incline, grub.Rotation.Forward.Angle( tr.Normal ) - 90f, 0.25f );
		grub.SetAnimParameter( "incline", _incline );
		grub.SetAnimParameter( "heightdiff", tr.Distance );

		var holdPose = HoldPose.None;
		if ( grub.IsTurn && grub.ActiveWeapon is not null && ctrl.ShouldShowWeapon() )
			holdPose = grub.ActiveWeapon.HoldPose;
		grub.SetAnimParameter( "holdpose", (int)holdPose );
	}
}
