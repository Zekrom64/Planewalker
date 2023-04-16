using Planewaker.Mkdungeon.Lore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planewaker.Mkdungeon {

	public record DungeonLore {

		public required Location Location { get; init; }

		public required Purpose Purpose { get; init; }

		public required ConceptSet Concepts { get; init; }

	}

}
