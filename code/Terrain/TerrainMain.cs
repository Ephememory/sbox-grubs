﻿using System.IO;
using System.Text;
using Grubs.Utils;

namespace Grubs.Terrain;

/// <summary>
/// The main class for terrains.
/// </summary>
public sealed partial class TerrainMain : Entity
{
	public static TerrainMain Instance = null!;
	public static TerrainMap Current => Instance._current;

	/// <summary>
	/// The <see cref="TerrainMap"/> in the world.
	/// </summary>
	[Net]
	private TerrainMap _current { get; set; } = null!;

	private static List<TerrainModel> TerrainModels { get; set; } = new();

	public TerrainMain()
	{
		Instance = this;
		Transmit = TransmitType.Always;
	}

	public override void Spawn()
	{
		base.Spawn();

		Instance = this;

		if ( GameConfig.TerrainFile != string.Empty )
		{
			var terrainFile = GameConfig.TerrainFile;
			BinaryReader? reader = null;
			try
			{
				if ( FileSystem.Mounted.FileExists( terrainFile ) )
				{
					reader = new BinaryReader( FileSystem.Mounted.OpenRead( terrainFile ) );
					_current = new TerrainMap( PremadeTerrain.Deserialize( reader ) );
				}
				else if ( FileSystem.Data.FileExists( terrainFile ) )
				{
					reader = new BinaryReader( FileSystem.Data.OpenRead( terrainFile ) );
					_current = new TerrainMap( PremadeTerrain.Deserialize( reader ) );
				}
				else
				{
					Log.Error( $"Map \"{terrainFile}\" does not exist. Reverting to random gen" );
					_current = new TerrainMap( Rand.Int( 99999 ) );
				}
			}
			catch( Exception e )
			{
				Log.Error( e );
			}
			finally
			{
				reader?.Close();
			}
		}
		else
			_current = new TerrainMap( Rand.Int( 99999 ) );

		Initialize();
	}

	/// <summary>
	/// Initializes the terrain models.
	/// </summary>
	public static void Initialize()
	{
		foreach ( var model in TerrainModels )
			model.Delete();
		TerrainModels.Clear();

		foreach ( var chunk in Current.TerrainGridChunks )
		{
			var terrainModel = new TerrainModel( chunk );
			TerrainModels.Add( terrainModel );
		}
	}

	/// <summary>
	/// Refreshes any chunks that have changed and need re-making.
	/// </summary>
	public static void RefreshDirtyChunks()
	{
		var chunks = Current.TerrainGridChunks;
		for ( var i = 0; i < chunks.Count; i++ )
		{
			var chunk = chunks[i];
			if ( !chunk.IsDirty )
				continue;

			TerrainModels[i].DestroyMeshAndCollision();
			TerrainModels[i].Delete();
			TerrainModels[i] = new TerrainModel( chunk );
			chunk.IsDirty = false;
		}
	}
}
