using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planewaker.Mkdungeon.Lore {

	public record Alignment {

		public static Alignment LawfulGood { get; } = new() {
			Name = "Lawful Good",
			IsLawful = true,
			IsGood = true,
			Concepts = new ConceptSet() {
				Concept.Lawful,
				Concept.Good
			}
		};

		public static Alignment LawfulNeutral { get; } = new() {
			Name = "Lawful Neutral",
			IsLawful = true,
			Concepts = new ConceptSet() { Concept.Lawful }
		};

		public static Alignment LawfulEvil { get; } = new() {
			Name = "Lawful Evil",
			IsLawful = true,
			IsEvil = true,
			Concepts = new ConceptSet() {
				Concept.Lawful,
				Concept.Evil
			}
		};

		public static Alignment NeutralGood { get; } = new() { Name = "Neutral Good", IsGood = true };

		public static Alignment TrueNeutral { get; } = new() { Name = "True Neutral", };

		public static Alignment NeutralEvil { get; } = new() { Name = "Neutral Evil", IsEvil = true };

		public static Alignment ChaoticGood { get; } = new() { Name = "Chaotic Good", IsChaotic = true, IsGood = true };

		public static Alignment ChaoticNeutral { get; } = new() { Name = "Chaotic Neutral", IsChaotic = true };

		public static Alignment ChaoticEvil { get; } = new() { Name = "Chaotic Evil", IsChaotic = true, IsEvil = true };

		public static IReadOnlyList<Alignment> AllAlignments { get; } = new List<Alignment>() { LawfulGood, LawfulNeutral, LawfulEvil, NeutralGood, TrueNeutral, NeutralEvil, ChaoticGood, ChaoticNeutral, ChaoticEvil };

		public bool IsLawful { get; init; } = false;

		public bool IsChaotic { get; init; } = false;

		public bool IsEvil { get; init; } = false; 

		public bool IsGood { get; init; } = false;

		public required string Name { get; init; }

		public required ConceptSet Concepts { get; init; }

		private Alignment() { }

		public static Alignment DeduceAlignment(IReadOnlySet<Concept> concepts) {
			bool good = concepts.Contains(Concept.Good);
			bool evil = concepts.Contains(Concept.Evil);
			bool lawful = concepts.Contains(Concept.Lawful);
			bool chaotic = concepts.Contains(Concept.Chaotic);

			if (good && evil) {
				good = false;
				evil = false;
			}
			if (lawful && chaotic) {
				lawful = false;
				chaotic = false;
			}

			if (good) {
				if (lawful) return LawfulGood;
				else if (chaotic) return ChaoticGood;
				else return NeutralGood;
			} else if (evil) {
				if (lawful) return LawfulEvil;
				else if (chaotic) return ChaoticEvil;
				else return NeutralEvil;
			} else {
				if (lawful) return LawfulNeutral;
				else if (chaotic) return ChaoticNeutral;
				else return TrueNeutral;
			}
		}

	}

}
