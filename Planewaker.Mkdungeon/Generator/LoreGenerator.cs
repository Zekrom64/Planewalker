using Planewaker.Mkdungeon.Lore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planewaker.Mkdungeon.Generator {
	public class LoreGenerator {

		public Dungeon Dungeon { get; }

		public Location? Location { get; set; } = null;

		public Purpose? Purpose { get; set; } = null;

		public LoreGenerator(Dungeon dungeon) {
			Dungeon = dungeon;
		}

		public DungeonLore Generate() {
			var rng = Dungeon.Random;
			ConceptSet concepts = new();

			Location ??= rng.Select(Location.AllLocations);
			concepts.UnionWith(Location.Concepts);

			Purpose ??= rng.Select(Purpose.AllPurposes);
			concepts.UnionWith(Purpose.Concepts);

			return new DungeonLore() {
				Location = Location,
				Purpose = Purpose,
				Concepts = concepts
			};
		}

	}
}
