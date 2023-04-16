using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planewaker.Mkdungeon.Lore {

	/// <summary>
	/// Enumeration of location biomes.
	/// </summary>
	public enum Biome {
		/// <summary>
		/// Generic biome with no specific features.
		/// </summary>
		Generic,
		/// <summary>
		/// Temperate forest or plains.
		/// </summary>
		Temperate,
		/// <summary>
		/// Cold artic or tundra.
		/// </summary>
		Cold,
		/// <summary>
		/// Arid hot desert.
		/// </summary>
		Desert,
		/// <summary>
		/// Temperate rocky terrain at high elevation.
		/// </summary>
		Alpine,
		/// <summary>
		/// Humid seaside or ocean with mild to warm temperatures.
		/// </summary>
		Ocean,
		/// <summary>
		/// Located outside the normal planes with no normal climate.
		/// </summary>
		Exoplanar,
		/// <summary>
		/// Humid inland with mild to warm temperatures.
		/// </summary>
		Tropical
	}

	/// <summary>
	/// A location gives more detail about where a dungeon is spatially.
	/// </summary>
	public record Location {

		/// <summary>
		/// The list of all known locations.
		/// </summary>
		public static List<Location> AllLocations { get; } = new() {
			new Location("inside a city building") {
				Concepts = new ConceptSet() {
					Concept.Urban,
					Concept.Civilized,
					Concept.Building,
					Concept.Small
				}
			},
			new Location("inside city sewers") {
				Concepts = new ConceptSet() {
					Concept.Urban,
					Concept.Civilized,
					Concept.Dirty
				}
			},
			new Location("beneath a farmhouse") {
				Concepts = new ConceptSet() {
					Concept.Civilized,
					Concept.Remote,
					Concept.Lush
				},
				Biome = Biome.Temperate
			},
			new Location("beneath a graveyard") {
				Concepts = new ConceptSet() {
					Concept.Death,
					Concept.Civilized,
					Concept.Holy
				}
			},
			new Location("beneath a ruined castle") {
				Concepts = new ConceptSet() {
					Concept.Civilized,
					Concept.Decay,
					Concept.Secure
				}
			},
			new Location("beneath a ruined city") {
				Concepts = new ConceptSet() {
					Concept.Civilized,
					Concept.Decay
				}
			},
			new Location("beneath a temple") {
				Concepts = new ConceptSet() {
					Concept.Holy,
					Concept.Building
				}
			},
			new Location("in a chasm") {
				Concepts = new ConceptSet() {
					Concept.Remote,
					Concept.Exotic
				},
				Biome = Biome.Desert
			},
			new Location("in a cliff face") {
				Concepts = new ConceptSet() {
					Concept.Remote,
					Concept.Exotic,
					Concept.Verticality
				},
				Biome = Biome.Alpine
			},
			new Location("in a desert") {
				Concepts = new ConceptSet() {
					Concept.Remote,
					Concept.Barren
				},
				Biome = Biome.Desert
			},
			new Location("in a forest") {
				Concepts = new ConceptSet() {
					Concept.Remote,
					Concept.Lush
				},
				Biome = Biome.Temperate
			},
			new Location("in a glacier") {
				Concepts = new ConceptSet() {
					Concept.Remote,
					Concept.Barren
				},
				Biome = Biome.Cold
			},
			new Location("in a gorge") {
				Concepts = new ConceptSet() {
					Concept.Remote,
					Concept.Verticality,
					Concept.Humid
				},
				Biome = Biome.Alpine
			},
			new Location("in a jungle") {
				Concepts = new ConceptSet() {
					Concept.Remote,
					Concept.Lush
				},
				Biome = Biome.Tropical
			},
			new Location("in a mountain pass") {
				Concepts = new ConceptSet() {
					Concept.Remote,
					Concept.Verticality
				},
				Biome = Biome.Alpine
			},
			new Location("in a swamp") {
				Concepts = new ConceptSet() {
					Concept.Remote,
					Concept.Wet,
					Concept.Lush,
					Concept.Dirty
				},
				Biome = Biome.Temperate
			},
			new Location("beneath a mesa") {
				Concepts = new ConceptSet() {
					Concept.Remote,
					Concept.Barren,
					Concept.Deep
				},
				Biome = Biome.Desert
			},
			new Location("in sea caves") {
				Concepts = new ConceptSet() {
					Concept.Natural,
					Concept.Wet
				},
				Biome = Biome.Ocean
			},
			new Location("in a mesa range") {
				Concepts = new ConceptSet() {
					Concept.Remote,
					Concept.Barren,
					Concept.Elevated
				},
				Biome = Biome.Desert
			},
			new Location("on a mountain peek") {
				Concepts = new ConceptSet() {
					Concept.Remote,
					Concept.Verticality,
					Concept.Barren,
					Concept.Elevated
				},
				Biome = Biome.Alpine
			},
			new Location("on a promontory") {
				Concepts = new ConceptSet() {
					Concept.Wet,
					Concept.Verticality,
					Concept.Temperate
				},
				Biome = Biome.Ocean
			},
			new Location("on an island") {
				Concepts = new ConceptSet() {
					Concept.Wet,
					Concept.Humid,
					Concept.Temperate,
					Concept.Remote
				},
				Biome = Biome.Ocean
			},
			new Location("underwater") {
				Concepts = new ConceptSet() {
					Concept.Wet,
					Concept.Remote,
					Concept.Deep,
					Concept.Remote,
					Concept.Exotic
				},
				Biome = Biome.Ocean
			},
			// Very exotic locations
			new Location("among the branches of a great tree") {
				Concepts = new ConceptSet() {
					Concept.Exotic,
					Concept.Remote,
					Concept.Lush,
					Concept.Temperate,
					Concept.Elevated,
					Concept.Extreme,
					Concept.Natural,
					Concept.Small
				},
				Biome = Biome.Temperate
			},
			new Location("around a geyser") {
				Concepts = new ConceptSet() {
					Concept.Exotic,
					Concept.Wet,
					Concept.Humid
				},
				Biome = Biome.Alpine
			},
			new Location("behind a waterfall") {
				Concepts = new ConceptSet() {
					Concept.Exotic,
					Concept.Wet,
					Concept.Humid
				}
			},
			new Location("buried in an avalanch") {
				Concepts = new ConceptSet() {
					Concept.Exotic,
					Concept.Elevated,
					Concept.Decay
				},
				Biome = Biome.Alpine
			},
			new Location("buried in a sandstorm") {
				Concepts = new ConceptSet() {
					Concept.Exotic,
					Concept.Decay,
					Concept.Hot
				},
				Biome = Biome.Desert
			},
			new Location("buried in volcanic ash") {
				Concepts = new ConceptSet() {
					Concept.Exotic,
					Concept.Elevated,
					Concept.Decay,
					Concept.Hot,
					Concept.Igneous
				},
				Biome = Biome.Alpine
			},
			new Location("sunken in a swamp") {
				Concepts = new ConceptSet() {
					Concept.Exotic,
					Concept.Temperate,
					Concept.Wet,
					Concept.Humid,
					Concept.Decay
				},
				Biome = Biome.Temperate
			},
			new Location("at the bottom of a sinkhole") {
				Concepts = new ConceptSet() {
					Concept.Exotic,
					Concept.Decay,
					Concept.Deep
				}
			},
			new Location("floating on the sea") {
				Concepts = new ConceptSet() {
					Concept.Exotic,
					Concept.Wet,
					Concept.Extreme,
					Concept.Elevated
				},
				Biome = Biome.Ocean
			},
			new Location("inside a meteorite") {
				Concepts = new ConceptSet() {
					Concept.Exotic,
					Concept.Extreme,
					Concept.Extraplanar,
					Concept.Small
				}
			},
			new Location("within an external plane") {
				Concepts = new ConceptSet() {
					Concept.Exotic,
					Concept.Extraplanar
				},
				Biome = Biome.Exoplanar
			},
			new Location("in an area devastated by a magical catastrophe") {
				Concepts = new ConceptSet() {
					Concept.Exotic,
					Concept.Magical,
					Concept.Extreme,
					Concept.Decay
				},
				Biome = Biome.Generic
			},
			new Location("on a cloud") {
				Concepts = new ConceptSet() {
					Concept.Exotic,
					Concept.Extreme,
					Concept.Elevated,
					Concept.Small
				}
			},
			new Location("in the Feywild") {
				Concepts = new ConceptSet() {
					Concept.Exotic,
					Concept.Fey,
					Concept.Magical,
					Concept.Lush
				},
				Biome = Biome.Exoplanar
			},
			new Location("in the Shadowfell") {
				Concepts = new ConceptSet() {
					Concept.Exotic,
					Concept.Magical,
					Concept.Fell,
					Concept.Death,
					Concept.Decay
				},
				Biome = Biome.Exoplanar
			},
			new Location("on an island in an underground sea") {
				Concepts = new ConceptSet() {
					Concept.Exotic,
					Concept.Wet,
					Concept.Extreme,
					Concept.Deep
				}
			},
			new Location("in a volcano") {
				Concepts = new ConceptSet() {
					Concept.Exotic,
					Concept.Extreme,
					Concept.Verticality,
					Concept.Hot,
					Concept.Igneous
				},
				Biome = Biome.Alpine
			},
			new Location("on the back of a Gargantuan living creature") {
				Concepts = new ConceptSet() {
					Concept.Exotic,
					Concept.Extreme,
					Concept.Small,
					Concept.Organic
				}
			},
			new Location("sealed within a magical dome of force") {
				Concepts = new ConceptSet() {
					Concept.Exotic,
					Concept.Magical,
					Concept.Small
				}
			},
			new Location("inside a Magnificent Mansion of Mordenkainen") {
				Concepts = new ConceptSet() {
					Concept.Exotic,
					Concept.Extraplanar,
					Concept.Magical,
					Concept.Civilized
				},
				Biome = Biome.Exoplanar
			}
		};

		/// <summary>
		/// A description of the location.
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// The biome the location is in.
		/// </summary>
		public Biome Biome { get; init; } = Biome.Generic;

		/// <summary>
		/// The concepts associated with this location.
		/// </summary>
		public ConceptSet Concepts { get; init; } = ConceptSet.Empty;

		public Location(string description) {
			Description = description;
		}

	}

}
