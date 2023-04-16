using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract.Core.Collections;
using Tesseract.Core.Numerics;

namespace Planewaker.Mkdungeon.Layout {

	/// <summary>
	/// Stores information about a single Z-layer of the dungeon.
	/// </summary>
	public class Layer {

		/// <summary>
		/// The index of the layer in the dungeon. Layer 0 is the 'ground floor' of the dungeon. Positive layers are
		/// higher up and negative layers are lower down.
		/// </summary>
		public required int Index { get; init; }

		/// <summary>
		/// The dungeon this layer is part of.
		/// </summary>
		public required Dungeon Dungeon { get; init; }

		// Grids of layer elements.
		internal readonly Grid2D<Tile?> grid = new();
		internal readonly Grid2D<Wall?> horizontalWalls = new();
		internal readonly Grid2D<Wall?> verticalWalls = new();

		/// <summary>
		/// Gets the tile on this layer at the given position.
		/// </summary>
		/// <param name="position">The position of the tile to get</param>
		/// <returns>The tile at the given position</returns>
		public Tile GetTile(Vector2i position) {
			Tile? tile = grid[position];
			if (tile == null) {
				tile = new Tile() { Layer = this, Position = position };
				grid[position] = tile;
			}
			return tile;
		}

		internal Wall GetWall(Vector2i position, bool vertical) {
			Grid2D<Wall?> grid = vertical ? verticalWalls : horizontalWalls;
			Wall? wall = grid[position];
			if (wall == null) {
				wall = new Wall() { Layer = this, Position = position, IsVertical = vertical };
				grid[position] = wall;
			}
			return wall;
		}

	}

}
