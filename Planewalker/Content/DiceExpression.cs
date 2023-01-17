using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planewalker.Content {

	public enum DiceOperation {
		Constant,
		Roll,
		Add,
		Subtract,
		Explode,
		Maximum,
		Minimum,
		Push,
		Pop
	}

	public enum RandomNumberMode {
		Strict,
		Relaxed
	}



	public record class DiceExpression {

		public static RandomNumberMode RandomMode { get; set; } = RandomNumberMode.Strict;

		public DiceOperation Operation { get; }

		public int FirstConstant { get; private init; }

		public int SecondConstant { get; private init; }

		public DiceExpression? Previous { get; private set; }

		public DiceExpression? Next { get; private set; }

		public int? Result { get; private set; }

		private DiceExpression(DiceOperation operation) {
			Operation = operation;
		}

		private static DiceExpression Parse(string chars, ref int offset) {
			char? PollNext(ref int offset) => offset >= chars.Length ? null : chars[offset++];

			char Next(ref int offset) => PollNext(ref offset) ?? throw new FormatException("Unexpected end of expression");

			int NextInt(ref int offset) {
				if (offset >= chars.Length) throw new FormatException("Expected a number");
				int start = offset;
				while (offset < chars.Length && char.IsDigit(chars[offset])) offset++;
				return int.Parse(chars[start..offset]);
			}

			char c = Next(ref offset);
			if (char.IsDigit(c)) {
				offset--;
				int firstNum = NextInt(ref offset);
				if (PollNext(ref offset) == 'd') {
					int secondNum = NextInt(ref offset);
					return new DiceExpression(DiceOperation.Roll) { FirstConstant = firstNum, SecondConstant = secondNum };
				} else return new DiceExpression(DiceOperation.Constant) { FirstConstant = firstNum };
			}

			return c switch {
				'd' => new DiceExpression(DiceOperation.Roll) { FirstConstant = 1, SecondConstant = NextInt(ref offset) },
				'+' => new DiceExpression(DiceOperation.Add),
				'-' => new DiceExpression(DiceOperation.Subtract),
				'!' => new DiceExpression(DiceOperation.Explode),
				'^' => new DiceExpression(DiceOperation.Maximum),
				'_' => new DiceExpression(DiceOperation.Minimum),
				'(' => new DiceExpression(DiceOperation.Push),
				')' => new DiceExpression(DiceOperation.Pop),
				_ => throw new FormatException($"Unknown token '{c}'"),
			};
		}

		public static DiceExpression Parse(string str) {
			int offset = 0;
			DiceExpression? initExpr = null;
			DiceExpression? prevExpr = null;
			while(offset < str.Length) {
				DiceExpression expr = Parse(str, ref offset);
				initExpr ??= expr;
				if (prevExpr != null) prevExpr.Next = expr;
				expr.Previous = prevExpr;
				prevExpr = expr;
			}
			return initExpr ?? throw new FormatException("Empty expression");
		}

	}

}
