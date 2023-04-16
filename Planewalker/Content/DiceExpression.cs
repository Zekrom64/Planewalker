using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract.ImGui;

namespace Planewalker.Content {

	public enum RandomNumberMode {
		Strict,
		Relaxed
	}

	public enum DiceOperation {
		Constant,
		Roll,
		Explode,
		Add,
		Subtract,
		Maximum,
		Minimum,
		BeginSubExpression,
		EndSubExpression,
		Variable
	}

	public struct DiceToken {

		public DiceOperation Operation;

		public int IntValue1 = default;

		public int IntValue2 = default;

		public string StringValue = string.Empty;

		public int Start;

		public int End;
		
		public DiceToken(DiceOperation op) {
			Operation = op;
		}

		public override string ToString() {
			return Operation switch {
				DiceOperation.Constant => IntValue1.ToString(),
				DiceOperation.Roll => $"{IntValue1}d{IntValue2}",
				DiceOperation.Explode => $"{IntValue1}d{IntValue2}!",
				DiceOperation.Add => "+",
				DiceOperation.Subtract => "-",
				DiceOperation.Maximum => "max",
				DiceOperation.Minimum => "min",
				DiceOperation.BeginSubExpression => "(",
				DiceOperation.EndSubExpression => ")",
				DiceOperation.Variable => $"${{{StringValue}}}",
				_ => throw new InvalidOperationException(),
			};
		}

	}

	public class DiceExpression {

		public IReadOnlyList<DiceToken> Tokens { get; }

		public string RawText { get; }

		private static void Validate(IReadOnlyList<DiceToken> tokens) {
			bool evalHasValue = false, evalHasOperator = false;
			int subCount = 0;
			
			int offset = 0;
			foreach (var token in tokens) {
				offset = token.Start;
				switch (token.Operation) {
					// Number-generating tokens
					case DiceOperation.Constant:
					case DiceOperation.Variable:
					case DiceOperation.Roll:
					case DiceOperation.Explode:
						if (evalHasValue) throw new FormatException($"Error at {offset}: Expected an operator");
						evalHasValue = true;
						evalHasOperator = false;
						if (token.Operation == DiceOperation.Roll) {
							if (token.IntValue1 < 1) throw new FormatException($"Error at {offset}: At least one die must be rolled");
							if (token.IntValue2 < 2) throw new FormatException($"Error at {offset}: Dice must have two or more sides");
						}
						break;
					// Number-operating tokens
					case DiceOperation.Add:
					case DiceOperation.Subtract:
					case DiceOperation.Maximum:
					case DiceOperation.Minimum:
						if (evalHasOperator) throw new FormatException($"Error at {offset}: Expected a value before an operator");
						if (!evalHasValue) throw new FormatException($"Error at {offset}: Operator must be preceded by a value");
						evalHasValue = false;
						evalHasOperator = true;
						break;
					case DiceOperation.BeginSubExpression:
						subCount++;
						evalHasValue = false;
						evalHasOperator = false;
						break;
					case DiceOperation.EndSubExpression:
						if (subCount <= 0) throw new FormatException($"Error at {offset}: Missing matching ')'");
						if (!evalHasValue) throw new FormatException($"Error at {offset}: Sub-expression must have a value");
						evalHasValue = true;
						subCount--;
						break;
				}
			}
			if (evalHasOperator) throw new FormatException($"Error at {offset}: Expected a value after an operator");
			if (subCount > 0) throw new FormatException($"Error at {offset}: Missing matching ')'");
		}

		private static DiceToken ParseToken(string chars, ref int offset) {
			char? PollNext(ref int offset) => offset >= chars.Length ? null : chars[offset++];

			char Next(ref int offset) => PollNext(ref offset) ?? throw new FormatException("Unexpected end of expression");

			int NextInt(ref int offset) {
				if (offset >= chars.Length) throw new FormatException("Expected a number");
				int start = offset;
				while (offset < chars.Length && char.IsDigit(chars[offset])) offset++;
				if (start == offset) throw new FormatException("Expected a number");
				return int.Parse(chars[start..offset]);
			}

			bool ShouldExplode(ref int offset) {
				char? next = PollNext(ref offset);
				if (next == null) return false;
				else if (next == '!') return true;
				else {
					offset--;
					return false;
				}
			}

			string NextVarName(ref int offset) {
				if (Next(ref offset) != '{') throw new FormatException("Expected '{' after '$'");
				int start = offset;
				char c;
				do {
					c = Next(ref offset);
					if (!char.IsLetterOrDigit(c) && c != '}') throw new FormatException($"Invalid character '{c}' for variable name");
				} while (c != '}');
				return chars[start..(offset - 1)];
			}

			char c = Next(ref offset);
			bool minus = false;
			if (c == '-') {
				minus = true;
				c = Next(ref offset);
			}
			if (char.IsDigit(c)) {
				offset--;
				int firstNum = NextInt(ref offset);
				if (minus) firstNum = -firstNum;
				char? next = PollNext(ref offset);
				if (next == 'd') {
					int secondNum = NextInt(ref offset);
					return new DiceToken(ShouldExplode(ref offset) ? DiceOperation.Explode : DiceOperation.Roll) { IntValue1 = firstNum, IntValue2 = secondNum };
				} else {
					if (next != null) offset--;
					return new DiceToken(DiceOperation.Constant) { IntValue1 = firstNum };
				}
			}

			return c switch {
				'd' => new DiceToken(DiceOperation.Roll) { IntValue1 = 1, IntValue2 = NextInt(ref offset), Operation = ShouldExplode(ref offset) ? DiceOperation.Explode : DiceOperation.Roll },
				'+' => new DiceToken(DiceOperation.Add),
				'-' => new DiceToken(DiceOperation.Subtract),
				'^' => new DiceToken(DiceOperation.Maximum),
				'_' => new DiceToken(DiceOperation.Minimum),
				'(' => new DiceToken(DiceOperation.BeginSubExpression),
				')' => new DiceToken(DiceOperation.EndSubExpression),
				'$' => new DiceToken(DiceOperation.Variable) { StringValue = NextVarName(ref offset) },
				'!' => throw new FormatException("Explode token '!' must come after a dice roll"),
				_ => throw new FormatException($"Unknown token '{c}'"),
			};
		}

		private static List<DiceToken> ParseTokens(string str) {
			List<DiceToken> tokens = new(16);
			// Parse tokens
			int offset = 0;
			while (offset < str.Length) {
				int start = offset;
				try {
					var token = ParseToken(str, ref offset);
					token.Start = start;
					token.End = offset;
					tokens.Add(token);
				} catch (Exception ex) {
					throw new FormatException($"Error at {offset}: {ex.Message}");
				}
			}
			if (tokens.Count == 0) throw new FormatException("Empty expression");
			return tokens;
		}

		public DiceExpression(string str) : this(ParseTokens(str)) { }

		public DiceExpression(IReadOnlyList<DiceToken> tokens) {
			// Validate tokens
			Validate(tokens);
			// Copy list
			Tokens = new List<DiceToken>(tokens);
			// Concatenate to generate raw text
			RawText = string.Concat(tokens.Select(token => token.ToString()));
		}

		public override string ToString() {
			return RawText;
		}

		public override bool Equals(object? obj) => obj is DiceExpression expr && RawText == expr.RawText;

		public override int GetHashCode() => RawText.GetHashCode();


		private static CancellationTokenSource randomModeCancellation = new();
		private static RandomNumberMode randomMode = RandomNumberMode.Strict;

		/// <summary>
		/// The random number generation mode to use when evaluating dice expressions.
		/// </summary>
		public static RandomNumberMode RandomMode {
			get => randomMode;
			set {
				if (randomMode != value) {
					randomMode = value;
					randomModeCancellation.Cancel();
					randomModeCancellation = new();
				}
			}
		}

		/// <summary>
		/// Rolls a die using the current random number mode.
		/// </summary>
		/// <param name="numSides">The number of sides on the die</param>
		/// <returns>The number rolled on the die</returns>
		public static async Task<int> RollDie(int numSides) {
			if (RandomMode == RandomNumberMode.Strict) {
				CancellationToken ct = randomModeCancellation.Token;
				Task<int> strictRandom = RandomSource.NextInt(numSides, ct);
				while (!strictRandom.IsCompleted) await Task.Delay(100, ct);
				if (strictRandom.IsCompletedSuccessfully) return strictRandom.Result + 1;
			}
			return RandomSource.NextIntRelaxed(numSides) + 1;
		}

		private struct EvalFrame {

			public int Accumulator;

			public DiceOperation? NextOperation;

		}

		public async Task<DiceResult> Evaluate() {
			EvalFrame frame = new();
			Stack<EvalFrame> frameStack = new();

			StringBuilder display = new();

			void AcceptValue(int value) {
				if (frame.NextOperation == null) frame.Accumulator = value;
				else {
					switch(frame.NextOperation) {
						case DiceOperation.Add:
							frame.Accumulator += value;
							break;
						case DiceOperation.Subtract:
							frame.Accumulator -= value;
							break;
						case DiceOperation.Maximum:
							frame.Accumulator = Math.Max(frame.Accumulator, value);
							break;
						case DiceOperation.Minimum:
							frame.Accumulator = Math.Min(frame.Accumulator, value);
							break;
						default:
							throw new InvalidOperationException();
					}
				}
			}

			int partialResult;
			foreach(var token in Tokens) {
				switch (token.Operation) {
					case DiceOperation.Constant:
						display.Append(token.IntValue1);
						AcceptValue(token.IntValue1);
						break;
					case DiceOperation.Variable:
						// TODO: Read value from current character
						AcceptValue(0);
						display.Append(token.ToString()).Append(" [ 0 ]");
						break;
					case DiceOperation.Roll:
						partialResult = 0;
						display.Append(token.ToString()).Append(" [ ");
						for (int i = 0; i < token.IntValue1; i++) {
							int roll = await RollDie(token.IntValue2);
							display.Append(roll).Append(", ");
							partialResult += roll;
						}
						display.Remove(display.Length - 2, 1);
						display.Append(']');
						AcceptValue(partialResult);
						break;
					case DiceOperation.Explode:
						partialResult = 0;
						display.Append(token.ToString()).Append(" [ ");
						for (int i = 0; i < token.IntValue1; i++) {
							int roll;
							do {
								roll = await RollDie(token.IntValue2);
								display.Append(roll);
								if (roll == token.IntValue2) display.Append('!');
								display.Append(", ");
								partialResult += roll;
							} while (roll == token.IntValue2);
						}
						display.Remove(display.Length - 2, 1);
						display.Append(']');
						AcceptValue(partialResult);
						break;
					case DiceOperation.Add:
					case DiceOperation.Subtract:
					case DiceOperation.Maximum:
					case DiceOperation.Minimum:
						display.Append(token.ToString());
						frame.NextOperation = token.Operation;
						break;
					case DiceOperation.BeginSubExpression:
						display.Append('(');
						frameStack.Push(frame);
						frame = new();
						break;
					case DiceOperation.EndSubExpression:
						display.Append(')');
						partialResult = frame.Accumulator;
						frame = frameStack.Pop();
						AcceptValue(partialResult);
						break;
				}
				display.Append(' ');
			}

			display.Append("= ").Append(frame.Accumulator);

			return new DiceResult(this, frame.Accumulator, display.ToString());
		}

	}

	public readonly struct DiceResult {

		public DiceExpression Expression { get; }

		public int Total { get; }

		public string DisplayText { get; }

		public DiceResult(DiceExpression expr, int total, string displayText) {
			Expression = expr;
			Total = total;
			DisplayText = displayText;
		}

	}

	public static class DiceStatTracker {

		private static readonly int[] trackedDice = new int[] { 4, 6, 8, 10, 12, 20, 100 };
		private static readonly Dictionary<int, int[]> resultMap = new Dictionary<int, int[]>();
		
		private static int[] GetResults(int die) {
			lock(resultMap) {
				if (resultMap.TryGetValue(die, out int[]? results)) return results;
				if (!trackedDice.Contains(die)) return Array.Empty<int>();
				results = new int[die];
				resultMap[die] = results;
				return results;
			}
		}

		public static void LogDiceRoll(int die, int result) {
			int[] results = GetResults(die);
			if (result >= results.Length) Interlocked.Increment(ref results[result - 1]);
		}

		public static void DrawHistograms() {
			var im = GImGui.Instance;
			foreach(int die in trackedDice) {
				if (im.CollapsingHeader($"d{die}")) {
					im.Indent();
					im.Unindent();
				}
			}
		}

	}

}
