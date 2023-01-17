using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Planewalker.Content {

	/// <summary>
	/// <para>The primary random number generator.</para>
	/// <para>
	/// This random number generator uses a combination of guarenteed True RNG provided by the
	/// <see href="https://csrc.nist.gov/projects/interoperable-randomness-beacons/beacon-20">NIST Randomness Beacon</see> and
	/// a locally initialized <see cref="RandomNumberGenerator"/> which <i>may</i> be pseudorandom but is cryptographically secure.
	/// The TRNG source produces genuine random numbers but produces them in blocks of 512 bits every minute and will broadcast these
	/// to everyone. To obfuscate the true random bits a local cryptographically secure PRNG is used to toggle bits via exclusive or both
	/// when a block of true random bits are received and a second time when random bits are requested. If there are leftover unused
	/// true random bits when a new block is received these are added to a 'stale' buffer to be used when the fresh random bits are
	/// exhausted. If both the fresh and stale buffers do not have enough random bits for a request the generator will wait until the
	/// next block of fresh bits is received before fulfilling the request.
	/// </para>
	/// <para>
	/// The random source also provides 'relaxed' variant methods which will fall back to the local PRNG if the fresh and stale buffers
	/// are exhausted. This may be preferred if a very large number of random numbers must be generated and it would be inconvenient to
	/// wait until enough true random bits arrive, at the potential cost of 'true' randomness.
	/// </para>
	/// </summary>
	public static class RandomSource {

		private static readonly HttpClient httpClient = new();

		private static readonly Uri nistLatestPulse = new("https://beacon.nist.gov/beacon/2.0/pulse/last");
		private static int lastPulseIndex = -1;
		
		private static readonly object randomLock = new();
		// The most recent true random bits 
		private static BigInteger trueRandomNumber;
		private static int trueRandomBits = 0;
		// A buffer of stale random bits 
		private static BigInteger staleRandomNumber;
		private static int staleRandomBits = 0;

		/// <summary>
		/// The number of available bits of entropy.
		/// </summary>
		public static int AvailableEntropy => trueRandomBits + staleRandomBits;

		private const int MaxStaleBits = 4096;
		private static readonly BigInteger staleNumberMask = (new BigInteger(1) << MaxStaleBits) - 1;

		private static readonly RandomNumberGenerator localRNG = RandomNumberGenerator.Create();

		// Polls the NIST webpage for new true random numbers, scheduling the next poll after 30 seconds
		private static async void PollNIST() {
			try {
				// Fetch the latest pulse from NIST
				var pulseResponse = await httpClient.SendAsync(new HttpRequestMessage() { RequestUri = nistLatestPulse });
				if (pulseResponse.IsSuccessStatusCode) {
					var pulse = JsonDocument.Parse(pulseResponse.Content.ReadAsStream());
					var jpulse = pulse.RootElement.GetProperty("pulse");
					// Make sure the response has a new pulse index
					int pulseIndex = jpulse.GetProperty("pulseIndex").GetInt32();
					if (pulseIndex != lastPulseIndex) {
						string srandom = jpulse.GetProperty("localRandomValue").GetString()!;
						// Generate a local unique 'hash' to combine with the true random numbers to hinder prediction
						byte[] prngBytes = new byte[64];
						localRNG.GetBytes(prngBytes);
						BigInteger hash = new(prngBytes);

						// Update the random value
						lock (randomLock) {
							// Push any remaining bits to the stale buffer
							if (trueRandomBits > 0) {
								staleRandomNumber <<= trueRandomBits;
								staleRandomNumber |= trueRandomNumber;
								staleRandomNumber &= staleNumberMask;
								staleRandomBits += trueRandomBits;
							}
							// Convert the acquired bits to a big integer
							trueRandomNumber = BigInteger.Parse(srandom, NumberStyles.HexNumber) ^ hash;
							trueRandomBits = 512;
						}

						// Update the most recent pulse index
						lastPulseIndex = pulseIndex;
					}
				}
			} catch (Exception ex) {
				Console.Error.WriteLine($"Failed to read NIST random pulse: {ex.Message}");
			}
			await Task.Delay(30000);
			_ = Task.Run(PollNIST);
		}

		static RandomSource() {
			PollNIST();
		}

		// Tries to pull the specified number of bits out of the given buffer
		private static bool TryPullBits(ref BigInteger buffer, ref int bitCount, int nbits, out int value) {
			if (nbits > bitCount) {
				value = 0;
				return false;
			}
			int mask = (1 << nbits) - 1;
			value = (int)(buffer & mask);
			buffer >>= nbits;
			bitCount -= nbits;
			return true;
		}

		/// <summary>
		/// Gets the next number of random bits, completing when all bits are available.
		/// </summary>
		/// <param name="nbits">The number of bits to read</param>
		/// <returns>Task with the result of the read bits.</returns>
		/// <exception cref="ArgumentException">If the number of bits requested is negative or larger than the size of an <see cref="int"/></exception>
		public static async Task<int> NextBits(int nbits) {
			if (nbits == 0) return 0;
			if (nbits < 0 || nbits > 32) throw new ArgumentException("Invalid number of bits to get", nameof(nbits));
			do {
				lock (randomLock) {
					// Try to use the fresh random bits first
					if (TryPullBits(ref trueRandomNumber, ref trueRandomBits, nbits, out int value)) return value ^ NextLocalBits(nbits);
					// Else try the stale bits
					if (TryPullBits(ref staleRandomNumber, ref staleRandomBits, nbits, out value)) return value ^ NextLocalBits(nbits);
				}
				// If we failed to acquire the bits wait a bit and try again
				await Task.Delay(100);
			} while (true);
		}

		// Gets a number of bits from the local pseudorandom number generator
		private static int NextLocalBits(int nbits) {
			Span<byte> bytes = stackalloc byte[4];
			localRNG.GetBytes(bytes);
			int mask = (1 << nbits) - 1;
			return BinaryPrimitives.ReadInt32LittleEndian(bytes) & mask;
		}

		/// <summary>
		/// Similar to <see cref="NextBits(int)"/> but will fallback to a pseudorandom number source provided by
		/// <see cref="RandomNumberGenerator"/> if the pool of truely random bits is exhausted.
		/// </summary>
		/// <param name="nbits">The number of bits to read</param>
		/// <returns>The read random bits</returns>
		/// <exception cref="ArgumentException">If the number of bits requested is negative or larger than the size of an <see cref="int"/></exception>
		public static int NextBitsRelaxed(int nbits) {
			if (nbits == 0) return 0;
			if (nbits < 0 || nbits > 32) throw new ArgumentException("Invalid number of bits to get", nameof(nbits));
			lock (randomLock) {
				// Try to use the fresh random bits first
				if (TryPullBits(ref trueRandomNumber, ref trueRandomBits, nbits, out int value)) return value ^ NextLocalBits(nbits);
				// Else try the stale bits
				if (TryPullBits(ref staleRandomNumber, ref staleRandomBits, nbits, out value)) return value ^ NextLocalBits(nbits);
			}
			return NextLocalBits(nbits);
		}

		/// <summary>
		/// <para>Gets the next random integer in the uniform range [0,max).</para>
		/// </summary>
		/// <param name="max">The maximum of the range, exclusive</param>
		/// <returns>Task with the result of the random integer within the range.</returns>
		public static async Task<int> NextInt(int max) {
			// Count the minimum number of bits required to encode the integer.
			int minBits = BitOperations.TrailingZeroCount(BitOperations.RoundUpToPowerOf2((uint)max));
			// If the range is an exact power of two just return the bits
			if (BitOperations.IsPow2(max)) return await NextBits(minBits);
			else { // Else we need a more complex sampling strategy
				   // Because this is a 'strict' context, do rejection sampling to guarentee a correct distribution
				int result;
				do {
					result = await NextBits(minBits);
				} while (result >= max);
				return result;
			}
		}

		/// <summary>
		/// Similar to <see cref="NextInt(int)"/> but will fallback to a pseudorandom number source if the pool of
		/// truely random bits is exhausted.
		/// </summary>
		/// <param name="max">The maximum of the range, exclusive</param>
		/// <returns>The random integer within the range</returns>
		public static int NextIntRelaxed(int max) {
			// Count the minimum number of bits required to encode the integer.
			int minBits = BitOperations.TrailingZeroCount(BitOperations.RoundUpToPowerOf2((uint)max));
			// Count the minimum number of bits required to encode the integer.
			if (BitOperations.IsPow2(max)) return NextBitsRelaxed(minBits);
			else { // Else we need a more complex sampling strategy
				// In a 'relaxed' context, we can sacrifice a little accuracy for performance
				// Try an initial random pattern, and rejection sample using the local RNG to toggle bits until valid
				int result = NextBitsRelaxed(minBits);
				while(result >= max) result ^= NextLocalBits(minBits);
				return result;
			}
		}

	}

}
