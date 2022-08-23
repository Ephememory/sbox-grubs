﻿using Grubs.Player;

namespace Grubs.States;

public class GameEndState : BaseState
{
	protected override void Enter( bool forced, params object[] parameters )
	{
		base.Enter( forced, parameters );

		if ( !IsServer )
			return;

		switch ( (GameResultType)parameters[0] )
		{
			case GameResultType.TeamWon:
				var playersWon = (GrubsPlayer[])parameters[1];
				break;
			case GameResultType.Draw:
				break;
			case GameResultType.Abandoned:
				var reason = (string)parameters[1];
				break;
			default:
				throw new ArgumentOutOfRangeException( nameof( parameters ) );
		}
	}
}
