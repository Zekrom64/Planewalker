namespace Planewaker.Mkdungeon.Lore {

	/// <summary>
	/// Accelerated structure for holding inferences between concepts. An inference measures how related
	/// two concepts are and is bidirectional between them (ie. A implies B as much as B implies A). An
	/// inference weight of 0 is the default and means two concepts have no correlation. A positive weight
	/// up to 1 means two concepts are correlated (one implies the other). A negative weight down to -1 means
	/// two objects have a negative correlation (one implies the absence of the other).
	/// </summary>
	public class InferenceSet {

		public static InferenceSet StandardInferences { get; } = new() {

		};

		// Weights mapped by source->target
		private readonly Dictionary<(Concept, Concept), float> inferences = new();
		// Edge lists mapped by source
		private readonly Dictionary<Concept, List<Concept>> edges = new();

		/// <summary>
		/// Adds an inference between two concepts with the given weight.
		/// </summary>
		/// <param name="first">First concept</param>
		/// <param name="second">Second concept</param>
		/// <param name="weight">The weight between the two concepts</param>
		public void Add(Concept first, Concept second, float weight) {
			inferences[(first, second)] = weight;
			inferences[(second, first)] = weight;

			if (!edges.TryGetValue(first, out List<Concept>? edgeList)) {
				edgeList = new List<Concept>();
				edges[first] = edgeList;
			}
			edgeList.Add(second);
			if (!edges.TryGetValue(second, out edgeList)) {
				edgeList = new List<Concept>();
				edges[second] = edgeList;
			}
			edgeList.Add(first);
		}

		/// <summary>
		/// Gets the weight of an inference directly between two concepts.
		/// </summary>
		/// <param name="first">First concept</param>
		/// <param name="second">Second concept</param>
		/// <returns>The weight between two concepts, or 0 if there is none</returns>
		public float GetWeight(Concept first, Concept second) {
			if (inferences.TryGetValue((first, second), out float weight)) return weight;
			else if (inferences.TryGetValue((second, first), out weight)) return weight;
			else return 0;
		}

		/// <summary>
		/// Gets the weight of a concept within the given set of concepts based on the current set of inferences.
		/// If the concept already exists within the set it will have the maximum weight of 1. If there are no
		/// direct inferences to concepts within the set the weight will be 0.
		/// </summary>
		/// <param name="concept"></param>
		/// <param name="set"></param>
		/// <returns></returns>
		public float GetWeight(Concept concept, IReadOnlyCollection<Concept> set) {

		}

	}

}
