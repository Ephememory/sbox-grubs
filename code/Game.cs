global using Sandbox;
global using Sandbox.UI;
global using Sandbox.UI.Construct;

global using System;
global using System.Collections.Generic;
global using System.Linq;

using Grubs.Player;
using Grubs.States;
using Grubs.Terrain;
using Grubs.UI;
using Grubs.Utils.Event;

namespace Grubs;

public partial class GrubsGame : Game
{
	public new static GrubsGame Current => Game.Current as GrubsGame;

	[Net]
	public BaseState CurrentState { get; set; }

	public TerrainMap TerrainMap { get; set; }

	public GrubsGame()
	{
		// Uncomment below to use WIP Terrain!
		// InitializeTerrainMap();

		if ( IsServer )
		{
			CurrentState = new WaitingState();
			_ = new EventRunner();
		}

		if ( IsClient )
		{
			_ = new Hud();
		}
	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		CurrentState?.ClientJoined( client );
	}

	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		CurrentState?.ClientDisconnected( cl, reason );
	}

	[Event.Tick]
	private void Tick()
	{
		CurrentState?.Tick();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		CurrentState?.Simulate( cl );
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		CurrentState?.FrameSimulate( cl );
	}

	public void InitializeTerrainMap()
	{
		TerrainMap = new TerrainMap( 100, 100 );

		var model = new TerrainModel();
	}
}
