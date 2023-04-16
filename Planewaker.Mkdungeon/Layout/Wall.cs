using Tesseract.Core.Numerics;

namespace Planewaker.Mkdungeon.Layout {
	public record Wall {

		public required Layer Layer { get; init; }

		public required bool IsVertical { get; init; }

		public required Vector2i Position { get; init; }

		internal Wall() { }

	}

}
