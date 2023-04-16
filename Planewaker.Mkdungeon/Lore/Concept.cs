using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Planewaker.Mkdungeon.Lore {

	/// <summary>
	/// Enumeration of lore concepts. A concept is just a word which acts as a hint 
	/// </summary>
	public enum Concept {
		//================//
		// Biome concepts //
		//================//
		/// <summary>
		/// Environment climate is temperate.
		/// </summary>
		Temperate,
		/// <summary>
		/// Environment climate is cold.
		/// </summary>
		Cold,
		/// <summary>
		/// Environment climate is hot.
		/// </summary>
		Hot,
		/// <summary>
		/// Environment is lush with vegetation.
		/// </summary>
		Lush,
		/// <summary>
		/// Environment is barren with vegitation.
		/// </summary>
		Barren,
		/// <summary>
		/// Environment is humid.
		/// </summary>
		Humid,
		/// <summary>
		/// Environment is wet/moist.
		/// </summary>
		Wet,
		/// <summary>
		/// Environment is extraplanar (outside the material plane).
		/// </summary>
		Extraplanar,

		//===================//
		// Location concepts //
		//===================//
		/// <summary>
		/// Location is urban (within a settlement).
		/// </summary>
		Urban,
		/// <summary>
		/// Location is 'civilized' (inhabited by some form of society).
		/// </summary>
		Civilized,
		/// <summary>
		/// Location is remote (distant from an urban center).
		/// </summary>
		Remote,
		/// <summary>
		/// Location is exotic/unusual.
		/// </summary>
		Exotic,
		/// <summary>
		/// Location has religious connotations (positive or negative).
		/// </summary>
		Holy,
		/// <summary>
		/// Location is deep below surface level.
		/// </summary>
		Deep,
		/// <summary>
		/// Location is dirty.
		/// </summary>
		Dirty,
		/// <summary>
		/// Location is decaying.
		/// </summary>
		Decay,
		/// <summary>
		/// Location is within an exterior building.
		/// </summary>
		Building,
		/// <summary>
		/// Location perfers vertical construction.
		/// </summary>
		Verticality,
		/// <summary>
		/// Location has ambient magic.
		/// </summary>
		Magical,
		/// <summary>
		/// Location is fey-aligned.
		/// </summary>
		Fey,
		/// <summary>
		/// Location is fell-aligned.
		/// </summary>
		Fell,
		/// <summary>
		/// Location 
		/// </summary>
		Organic,
		/// <summary>
		/// Location is elevated above surface level.
		/// </summary>
		Elevated,
		/// <summary>
		/// Location is igneous/volcanic.
		/// </summary>
		Igneous,
		/// <summary>
		/// Location is in an extreme environment.
		/// </summary>
		Extreme,
		/// <summary>
		/// Location is physically small.
		/// </summary>
		Small,

		//==================//
		// Purpose concepts //
		//==================//
		/// <summary>
		/// Purpose is to create death.
		/// </summary>
		Death,
		/// <summary>
		/// Purpose is habitation for monsters.
		/// </summary>
		Monster,
		/// <summary>
		/// Purpose is the home for some entity.
		/// </summary>
		Home,
		/// <summary>
		/// Purpose is to confuse an ensnare.
		/// </summary>
		Confusion,
		/// <summary>
		/// Purpose is industrial.
		/// </summary>
		Industry,
		/// <summary>
		/// Purpose is to house a portal.
		/// </summary>
		Portal,
		/// <summary>
		/// Purpose is to hold some kind of villain.
		/// </summary>
		Villain,
		/// <summary>
		/// Purpose is a tomb for some entity.
		/// </summary>
		Burial,
		/// <summary>
		/// Purpose is a vault to contain something.
		/// </summary>
		Vault,
		/// <summary>
		/// Purpose is to hold some form of treasure.
		/// </summary>
		Treasure,
		/// <summary>
		/// Purpose requires a complex structure.
		/// </summary>
		Complex,
		/// <summary>
		/// Purpose is to provide security for something.
		/// </summary>
		Secure,
		/// <summary>
		/// Purpose is for evil (per alignment).
		/// </summary>
		Evil,
		/// <summary>
		/// Purpose is for good (per alignment).
		/// </summary>
		Good,
		/// <summary>
		/// Purpose is lawful (per alignment).
		/// </summary>
		Lawful,
		/// <summary>
		/// Purpose is chaotic (per alignment).
		/// </summary>
		Chaotic,
		/// <summary>
		/// Purpose is necromantic in nature.
		/// </summary>
		Necrotic,
		/// <summary>
		/// Purpose is natural formation.
		/// </summary>
		Natural
	}

	/// <summary>
	/// An accelerated set which can hold one of each concept.
	/// </summary>
	public class ConceptSet : ISet<Concept>, IReadOnlySet<Concept> {

		private static readonly int fixedLength = Enum.GetValues<Concept>().Length;

		/// <summary>
		/// An empty accelerated concept set.
		/// </summary>
		public static ConceptSet Empty { get; } = new ConceptSet() { IsReadOnly = true };	

		// The bitmask of concepts in the set
		private readonly BitArray bitmask = new(fixedLength);

		public int Count { get; private set; } = 0;

		public bool IsReadOnly { get; init; }

		/// <summary>
		/// Creates a new empty concept set.
		/// </summary>
		public ConceptSet() { }

		/// <summary>
		/// Creates a copy of another set of concepts.
		/// </summary>
		/// <param name="copy">Set of concepts to copy</param>
		public ConceptSet(IEnumerable<Concept> copy) {
			if (copy is ConceptSet cs) {
				bitmask.Or(cs.bitmask);
				Count = cs.Count;
			} else {
				foreach (Concept c in copy) {
					bitmask[(int)c] = true;
					Count++;
				}
			}
		}

		public bool Add(Concept item) {
			if (IsReadOnly) throw new NotSupportedException();
			bool adding = !bitmask[(int)item];
			bitmask[(int)item] = true;
			if (adding) Count++;
			return adding;
		}

		public void Clear() {
			if (IsReadOnly) throw new NotSupportedException();
			bitmask.SetAll(false);
			Count = 0;
		}

		private void Recount() {
			int count = 0;
			for(int i = 0; i < bitmask.Length; i++)
				if (bitmask[i]) count++;
			Count = count;
		}

		public bool Contains(Concept item) => bitmask[(int)item];

		public void CopyTo(Concept[] array, int arrayIndex) {
			for (int i = 0; i < bitmask.Length; i++)
				if (bitmask[i]) array[arrayIndex++] = (Concept)i;
		}

		public void ExceptWith(IEnumerable<Concept> other) {
			if (IsReadOnly) throw new NotSupportedException();
			if (other is ConceptSet cs) {
				// AND with a mask of all elements not in the other set
				BitArray arr = new(bitmask.Length);
				arr.SetAll(true);
				arr.Xor(cs.bitmask);
				bitmask.And(arr);
				Recount();
			} else {
				foreach (Concept c in other) Remove(c);
			}
		}

		public IEnumerator<Concept> GetEnumerator() {
			if (Count != 0) {
				for (int i = 0; i < bitmask.Length; i++)
					if (bitmask[i]) yield return (Concept)i;
			}
		}

		public void IntersectWith(IEnumerable<Concept> other) {
			if (IsReadOnly) throw new NotSupportedException();
			if (other is ConceptSet cs) {
				bitmask.And(cs.bitmask);
				Recount();
			} else {
				for (int i = 0; i < bitmask.Length; i++)
					if (!other.Contains((Concept)i)) Remove((Concept)i);
			}
		}

		private bool TestSubset(IEnumerable<Concept> other, out int otherCount) {
			if (other is ConceptSet cs) {
				otherCount = cs.Count;
				// Bitwise AND between both sets
				BitArray arr = new(cs.bitmask);
				arr.And(bitmask);
				// Flip bits which are in the other set
				arr.Xor(bitmask);
				// If any bits are set there are elements in the other set which are not in this set
				for (int i = 0; i < arr.Length; i++)
					if (arr[i]) return false;
				return true;
			} else {
				int count = 0;
				bool subset = true;
				foreach(Concept c in other) {
					count++;
					if (!Contains(c)) {
						subset = false;
						break;
					}
				}
				otherCount = count;
				return subset;
			}
		}

		public bool IsProperSubsetOf(IEnumerable<Concept> other) => TestSubset(other, out int count) && count != Count;

		public bool IsSubsetOf(IEnumerable<Concept> other) => TestSubset(other, out _);

		public bool IsProperSupersetOf(IEnumerable<Concept> other) {
			throw new NotImplementedException();
		}

		public bool IsSupersetOf(IEnumerable<Concept> other) {
			throw new NotImplementedException();
		}

		public bool Overlaps(IEnumerable<Concept> other) {
			if (other is ConceptSet cs) {
				for(int i = 0; i < bitmask.Length; i++)
					if (bitmask[i] == cs.bitmask[i]) return true;
				return false;
			} else {
				foreach (Concept c in other)
					if (bitmask[(int)c]) return true;
				return false;
			}
		}

		public bool Remove(Concept item) {
			if (IsReadOnly) throw new NotSupportedException();
			bool contains = bitmask[(int)item];
			bitmask[(int)item] = false;
			if (contains) Count--;
			return contains;
		}

		public bool SetEquals(IEnumerable<Concept> other) {
			if (other is ConceptSet cs) {
				// XOR bitmasks, set will be equal if no bits are set
				BitArray arr = new(cs.bitmask);
				arr.Xor(bitmask);
				for (int i = 0; i < arr.Length; i++)
					if (arr[i]) return false;
				return true;
			} else {
				int count = Count;
				foreach (Concept c in other) {
					if (!Contains(c)) return false;
					else count--;
				}
				return count == 0;
			}
		}

		public void SymmetricExceptWith(IEnumerable<Concept> other) {
			if (IsReadOnly) throw new NotSupportedException();
			if (other is ConceptSet cs) {
				bitmask.Xor(cs.bitmask);
				Recount();
			} else {
				foreach (Concept c in other) {
					if (Contains(c)) Remove(c);
					else Add(c);
				}
			}
		}

		public void UnionWith(IEnumerable<Concept> other) {
			if (IsReadOnly) throw new NotSupportedException();
			if (other is ConceptSet cs) {
				bitmask.Or(cs.bitmask);
				Recount();
			} else {
				foreach (Concept c in other) Add(c);
			}
		}

		void ICollection<Concept>.Add(Concept item) => Add(item);

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	}

}
