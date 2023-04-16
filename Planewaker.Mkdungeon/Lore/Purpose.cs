using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planewaker.Mkdungeon.Lore {

	/// <summary>
	/// A purpose determines why the dungeon exists.
	/// </summary>
	public record Purpose {

		/// <summary>
		/// All possible purposes for a dungeon.
		/// </summary>
		public static readonly List<Purpose> AllPurposes = new() {
			new Purpose("death trap", "built to destroy anything that attempts enters it") {
				Concepts = new ConceptSet() {
					Concept.Death,
					Concept.Treasure
				}
			},
			new Purpose("lair", "a den for some monstrous creature") {
				Concepts = new ConceptSet() {
					Concept.Home,
					Concept.Monster
				}
			},
			new Purpose("maze", "a confusing labrynth which tricks intruders") {
				Concepts = new ConceptSet() {
					Concept.Confusion,
					Concept.Treasure,
					Concept.Monster
				}
			},
			new Purpose("mine", "an abandoned operation looking for something precious underground") {
				Concepts = new ConceptSet() {
					Concept.Industry,
					Concept.Deep,
					Concept.Civilized
				}
			},
			new Purpose("planar gate", "a place where the boundary between worlds is thin") {
				Concepts = new ConceptSet() {
					Concept.Extraplanar
				}
			},
			new Purpose("stronghold", "the base for some more powerful and organized force") {
				Concepts = new ConceptSet() {
					Concept.Villain,
					Concept.Home,
					Concept.Complex,
					Concept.Secure
				}
			},
			new Purpose("temple", "a place of worship to some deity") {
				Concepts = new ConceptSet() {
					Concept.Holy
				}
			},
			new Purpose("tomb", "the final resting place of the dead") {
				Concepts = new ConceptSet() {
					Concept.Death,
					Concept.Holy,
					Concept.Civilized
				}
			},
			new Purpose("vault", "a secure location for storing something valuable") {
				Concepts = new ConceptSet() {
					Concept.Treasure,
					Concept.Secure,
					Concept.Civilized,
					Concept.Vault
				}
			}
		};

		/// <summary>
		/// The name of the purpose.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// A description of the purpose.
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// The set of concepts associated with this purpose.
		/// </summary>
		public ConceptSet Concepts { get; init; } = ConceptSet.Empty;

		public Purpose(string name, string description) {
			Name = name;
			Description = description;
		}

	}

}
