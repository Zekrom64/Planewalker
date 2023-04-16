using Planewaker.Mkdungeon.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract.Core.Numerics.Random;

namespace Planewaker.Mkdungeon {

	public class Dungeon {

		/// <summary>
		/// The random number source for dungeon generation.
		/// </summary>
		public required IRandom Random { get; init; }

		private readonly Dictionary<int, Layer> layers = new();

		public Layer GetLayer(int index) {
			if (layers.TryGetValue(index, out Layer? layer)) return layer;
			layer = new Layer() { Dungeon = this, Index = index };
			layers[index] = layer;
			return layer;
		}

	}

}
