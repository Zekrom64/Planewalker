using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract.Core.Numerics;

namespace Planewaker.Mkdungeon.Layout {

	public abstract class Space {

		public required Dungeon Dungeon { get; init; }

		public abstract Recti BoundingBox { get; }

		public Layer PrimaryLayer { get; init; }

	}

}
