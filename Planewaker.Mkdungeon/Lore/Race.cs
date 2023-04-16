using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planewaker.Mkdungeon.Lore {

	public record Race {

		public static Race Beholder { get; } = new() {
			Name = "Beholder",
			StereotypeConcepts = new ConceptSet() {
				Concept.Civilized
			},
			Alignment = Alignment.LawfulEvil
		};

		public static Race Dwarf { get; } = new() {
			Name = "Dwarf",
			PluralName = "Dwarves",
			StereotypeConcepts = new ConceptSet() {
				Concept.Industry,
				Concept.Civilized
			}
		};

		public required string Name { get; init; }

		private string? pluralName = null;
		public string PluralName {
			get => pluralName ?? Name + "s";
			init => pluralName = value;
		}

		/// <summary>
		/// The general alignment of creatures of this race. Note that some 
		/// </summary>
		public Alignment Alignment { get; init; } = Alignment.TrueNeutral;

		public ConceptSet StereotypeConcepts { get; init; } = ConceptSet.Empty;

	}

}
