﻿using Grubs.Terrain.CSG;
using Sandbox.Csg;

namespace Grubs;

[Category( "Terrain" )]
public partial class World : Entity
{
	[Net]
	public CsgSolid CsgWorld { get; set; }

	[Net]
	public CsgSolid CsgBackground { get; set; }

	[Net]
	public DamageZone KillZone { get; set; }

	public CsgBrush CubeBrush { get; } = ResourceLibrary.Get<CsgBrush>( "brushes/cube.csg" );
	public CsgBrush CoolBrush { get; } = ResourceLibrary.Get<CsgBrush>( "brushes/cool.csg" );
	public CSGBrushPrefab PrefabBrush { get; set; }
	public CsgMaterial DefaultMaterial { get; } = ResourceLibrary.Get<CsgMaterial>( "materials/csg/default.csgmat" );
	public CsgMaterial AltSandMaterial { get; } = ResourceLibrary.Get<CsgMaterial>( "materials/csg/sand_a.csgmat" );
	public CsgMaterial SandMaterial { get; } = ResourceLibrary.Get<CsgMaterial>( "materials/csg/sand_b.csgmat" );
	public CsgMaterial LavaMaterial { get; } = ResourceLibrary.Get<CsgMaterial>( "materials/csg/lava.csgmat" );
	public CsgMaterial RockMaterial { get; } = ResourceLibrary.Get<CsgMaterial>( "materials/csg/rocks_a.csgmat" );

	public readonly float WorldLength = GrubsConfig.TerrainLength;
	public readonly float WorldHeight = GrubsConfig.TerrainHeight;
	public const float WorldWidth = 64f;

	private readonly float _zoom = GrubsConfig.TerrainNoiseZoom;
	private const float GridSize = 1024f;

	public override void Spawn()
	{
		Assert.True( Game.IsServer );

		Reset();
	}

	public void Reset()
	{
		CsgWorld?.Delete();
		CsgBackground?.Delete();

		CsgWorld = new CsgSolid( GridSize );


		CsgBackground = new CsgSolid( GridSize );

		if ( GrubsConfig.TerrainLevelType == 0 )
		{
			CsgWorld.Add( CubeBrush, SandMaterial, scale: new Vector3( WorldLength, WorldWidth, WorldHeight ), position: new Vector3( 0, 0, -WorldHeight / 2 ) );
			CsgBackground.Add( CubeBrush, RockMaterial, scale: new Vector3( WorldLength, WorldWidth, WorldHeight ), position: new Vector3( 0, 72, -WorldHeight / 2 ) );
			GenerateRandomWorld();
		}
		else
		{

			GenerateTextureWorld( "textures/texturelevels/" + GrubsConfig.TerrainLevelType.ToString().ToLower() + ".tlvl" );
		}
		SetupKillZone();

		/*if ( PrefabBrush is null )
		{
			PrefabBrush = CSGBrushPrefab.FromPrefab( "brushes/csgsphere_low.prefab" );
			PrefabBrush.GenerateBrush();
			Log.Info( "Loaded prefab brush!" );
			Log.Info( PrefabBrush.GeneratedBrush.ConvexSolids[0].Planes.Count + " planes" );
		}*/


	}

	public void SubtractDefault( Vector3 min, Vector3 max )
	{
		CsgWorld.Subtract( CoolBrush, (min + max) * 0.5f, max - min );//
		CsgWorld.Paint( CoolBrush, AltSandMaterial, (min + max) * 0.5f, (max - min) * 1.2f );//PrefabBrush.GeneratedBrush
	}

	public void AddDefault( Vector3 min, Vector3 max )
	{
		CsgWorld.Add( CoolBrush, SandMaterial, (min + max) * 0.5f, ((max - min) * 1.15f).WithY( 36 ) );
	}

	public void PaintDefault( Vector3 min, Vector3 max )
	{
		CsgWorld.Paint( CoolBrush, AltSandMaterial, (min + max) * 0.5f, (max - min) * 1.2f );
	}

	public void SubtractLine( Vector3 start, Vector3 stop, float size, Rotation rotation )
	{
		var midpoint = new Vector3( (start.x + stop.x) / 2, 0f, (start.z + stop.z) / 2 );
		var scale = new Vector3( Vector3.DistanceBetween( start, stop ), 64f, size );

		CsgWorld.Subtract( CubeBrush, midpoint, scale, Rotation.FromPitch( rotation.Pitch() ) );
		CsgWorld.Paint( CubeBrush, DefaultMaterial, midpoint, scale.WithZ( size * 1.1f ), Rotation.FromPitch( rotation.Pitch() ) );
	}

	private float[,] _terrainGrid;
	private readonly int _resolution = 16;

	public List<Vector3> PossibleSpawnPoints = new List<Vector3>();

	public void GenerateTextureWorld( string TexturePath )
	{
		ResourceLibrary.TryGet<TextureLevel>( TexturePath, out TextureLevel map );
		if ( map != null )
		{
			PossibleSpawnPoints.Clear();
			var _WorldLength = map.texture.Width * 16;
			var _WorldHeight = map.texture.Height * 16;
			var pointsX = map.texture.Width;
			var pointsZ = map.texture.Height;

			CsgWorld.Add( CubeBrush, SandMaterial, scale: new Vector3( _WorldLength, WorldWidth, _WorldHeight ), position: new Vector3( 0, 0, -_WorldHeight / 2 ) );
			CsgBackground.Add( CoolBrush, RockMaterial, scale: new Vector3( _WorldLength, WorldWidth, _WorldHeight ), position: new Vector3( 0, 72, -_WorldHeight / 2 ) );

			_terrainGrid = new float[pointsX, pointsZ];

			Color32[] pixels = map.texture.GetPixels().Reverse().ToArray();

			var min = new Vector3();
			var max = new Vector3();
			var n = 0;
			int index = 0;

			var paddedRes = 16;// + (16 * 0.5f);

			for ( var x = 0; x < pointsX; x++ )
			{
				for ( var z = 0; z < pointsZ; z++ )
				{
					index = z * pointsX + x;

					n = pixels[index].a;

					_terrainGrid[x, z] = n;

					// Add solid where alpha == 255
					if ( _terrainGrid[x, z] == 255 )
					{
						min = new Vector3( (x * 16) - paddedRes, -32, (z * 16) - paddedRes );
						max = new Vector3( (x * 16) + paddedRes, 32, (z * 16) + paddedRes );

						// Offset by position.
						min -= new Vector3( _WorldLength / 2, 0, _WorldHeight );
						max -= new Vector3( _WorldLength / 2, 0, _WorldHeight );
						//AddDefault( min, max );
						AddDefault( min, max );
					}
					else
					{
						min = new Vector3( (x * 16), -32, (z * 16) );
						max = new Vector3( (x * 16), 32, (z * 16) );

						// Offset by position.
						min -= new Vector3( _WorldLength / 2, 0, _WorldHeight );
						max -= new Vector3( _WorldLength / 2, 0, _WorldHeight );

						var avg = (min + max) / 2;
						PossibleSpawnPoints.Add( avg );
					}
				}
			}
		}
	}

	public void GenerateRandomWorld()
	{
		PossibleSpawnPoints.Clear();
		var pointsX = (WorldLength / _resolution).CeilToInt();
		var pointsZ = (WorldHeight / _resolution).CeilToInt();

		_terrainGrid = new float[pointsX, pointsZ];

		var r = Game.Random.Int( 99999 );

		// Initialize Simplex array of noise.
		for ( var x = 0; x < pointsX; x++ )
		{
			for ( var z = 0; z < pointsZ; z++ )
			{
				var n = Noise.Perlin( (x + r) * _zoom, r, (z + r) * _zoom );
				n = Math.Abs( (n * 2) - 1 );
				_terrainGrid[x, z] = n;

				// Subtract from the solid where the noise is under a certain threshold.
				if ( _terrainGrid[x, z] < 0.1f )
				{
					// Pad the subtraction so the subtraction is more clean.
					var paddedRes = _resolution + (_resolution * 0.75f);

					var min = new Vector3( (x * _resolution) - paddedRes, -32, (z * _resolution) - paddedRes );
					var max = new Vector3( (x * _resolution) + paddedRes, 32, (z * _resolution) + paddedRes );

					// Offset by position.
					min -= new Vector3( WorldLength / 2, 0, WorldHeight );
					max -= new Vector3( WorldLength / 2, 0, WorldHeight );
					SubtractDefault( min, max );

					var avg = (min + max) / 2;
					PossibleSpawnPoints.Add( avg );
				}
			}
		}
	}

	private void SetupKillZone()
	{
		var killBounds = new MultiShape().AddShape(
			BoxShape
			.WithSize( new Vector3( int.MaxValue, WorldWidth, 100 ) )
			.WithOffset( new Vector3( -int.MaxValue / 2, -WorldWidth / 2, -WorldHeight - 100 ) ) );

		KillZone = new DamageZone()
			.WithDamageTags( "outofarea" )
			.WithInstantKill( true )
			.WithDamage( 9999 )
			.WithPosition( Vector3.Zero )
			.WithShape( killBounds )
			.Finish<DamageZone>();
	}

	public Vector3 FindSpawnLocation()
	{
		int iterations = 0;
		while ( true && iterations < 10000 )
		{
			/*var x = Game.Random.Int( ((int)WorldLength / _resolution) - 1 );
			var z = Game.Random.Int( ((int)WorldHeight / _resolution) - 1 );
			if ( _terrainGrid[x, z] > 0.1f )
				continue;*/

			var startPos = Game.Random.FromList( PossibleSpawnPoints );//new Vector3( (x * _resolution) - WorldLength / 2, 0, (z * _resolution) - WorldHeight );
			var tr = Trace.Ray( startPos, startPos + Vector3.Down * WorldHeight ).WithTag( "solid" ).Run();
			if ( tr.Hit )
				return tr.EndPosition;
		}

		Log.Warning( "Couldn't find spawn location in 10,000 iterations." );
		return new Vector3( 0f );
	}

	[ConCmd.Admin( "gr_regen" )]
	public static void RegenWorld()
	{
		var world = GamemodeSystem.Instance.GameWorld;

		world.Reset();
	}
}
