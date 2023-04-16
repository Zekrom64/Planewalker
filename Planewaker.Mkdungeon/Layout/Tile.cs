using Tesseract.Core.Numerics;

namespace Planewaker.Mkdungeon.Layout {
	public record Tile {

		public required Layer Layer { get; init; }

		public required Vector2i Position { get; init; }

		internal Tile() { }

		private Wall[]? walls = null;

		public Wall GetWall(CardinalDirection direction) {
			if (walls == null) {
				walls = new Wall[4];
				foreach (var dir in Enum.GetValues<CardinalDirection>()) {
					Wall wall = dir switch {
						CardinalDirection.North => Layer.GetWall(Position, false),
						CardinalDirection.South => Layer.GetWall(Position + new Vector2i(0, 1), false),
						CardinalDirection.East => Layer.GetWall(Position + new Vector2i(1, 0), true),
						CardinalDirection.West => Layer.GetWall(Position, true),
						_ => throw new NotImplementedException(),
					};
					walls[(int)direction] = wall;
				}
			}
			return walls[(int)direction];
		}

		private Tile?[]? adjacent = null;

	}

}
