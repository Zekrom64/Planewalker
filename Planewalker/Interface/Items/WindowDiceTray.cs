using Planewalker.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Tesseract.ImGui;

namespace Planewalker.Interface.Items {

	public class ElementVisualDiceExpression {

		private static readonly DiceOperation[] operatorOps = new DiceOperation[] { DiceOperation.Add, DiceOperation.Subtract, DiceOperation.Maximum, DiceOperation.Minimum, DiceOperation.EndSubExpression };
		private static readonly string[] operatorOpsNames = new string[] { "add", "subtract", "maximum", "minimum", "return" };
		private static readonly DiceOperation[] valueOps = new DiceOperation[] { DiceOperation.Constant, DiceOperation.Variable, DiceOperation.Roll, DiceOperation.Explode, DiceOperation.BeginSubExpression };
		private static readonly string[] valueOpsNames = new string[] { "Constant", "Variable", "Roll", "Exploding Roll", "Sub-Expression" };

		private ElementVisualDiceExpression? previous = null;

		private ElementVisualDiceExpression? next = null;

		private DiceToken operatorToken = new(DiceOperation.Add);

		private DiceToken valueToken = new(DiceOperation.Constant);

		private readonly ImGuiTextBuffer valueStrBuffer = new();

		private int opIndex = 0;
		private int valueIndex = 0;

		public void Draw(ref int indent) {
			var im = GImGui.Instance;

			if (previous != null) {
				// Process the 'remove' button if not the first
				if (UI.ColoredButton($"-###remove{GetHashCode()}", new Vector4(0.8f, 0, 0, 1))) RemoveSelf();
				im.SameLine();
			}

			if (previous == null || previous.valueToken.Operation == DiceOperation.BeginSubExpression) {
				// If we're the first statement then generate 'Take the '
				im.Text("Take the "u8);
			} else {
				// Generate 'Then <operation>[ with the ]'
				im.Text("Then "u8);
				im.SameLine();
				im.SetNextItemWidth(100);
				if (im.Combo($"###operator{GetHashCode()}", ref opIndex, operatorOpsNames)) operatorToken.Operation = operatorOps[opIndex];
				// Only show the end text if not returning
				if (operatorToken.Operation != DiceOperation.EndSubExpression) {
					im.SameLine();
					im.Text(" with the "u8);
				}
			}

			// Only show value text if not returning
			if (operatorToken.Operation != DiceOperation.EndSubExpression) {
				im.SameLine();
				im.SetNextItemWidth(150);
				if (im.Combo($"###value{GetHashCode()}", ref valueIndex, valueOpsNames)) {
					valueToken.Operation = valueOps[valueIndex];
					// Reset the value token state
					switch(valueToken.Operation) {
						case DiceOperation.Constant:
							valueToken.IntValue1 = 0;
							break;
						case DiceOperation.Variable:
							valueStrBuffer.Clear();
							break;
						case DiceOperation.Roll:
						case DiceOperation.Explode:
							valueToken.IntValue1 = 1;
							valueToken.IntValue2 = 20;
							break;
					}
				}

				switch (valueToken.Operation) {
					case DiceOperation.Constant:
						im.SameLine();
						im.SetNextItemWidth(50);
						im.DragInt($"###constant{GetHashCode()}", ref valueToken.IntValue1, 0.5f, -100, 100);
						break;
					case DiceOperation.Variable:
						im.SameLine();
						im.SetNextItemWidth(200);
						if (im.InputText($"###text{GetHashCode()}", valueStrBuffer, ImGuiInputTextFlags.CallbackResize)) valueToken.StringValue = valueStrBuffer;
						break;
					case DiceOperation.Roll:
					case DiceOperation.Explode:
						im.SameLine();
						im.SetNextItemWidth(50);
						im.DragInt($"###diceCount{GetHashCode()}", ref valueToken.IntValue1, 0.5f, 1, 100);
						im.SameLine();
						im.Text("d"u8);
						im.SameLine();
						im.SetNextItemWidth(50);
						im.DragInt($"###diceSize{GetHashCode()}", ref valueToken.IntValue2, 0.5f, 2, 100);
						break;
					case DiceOperation.BeginSubExpression:
						im.Indent();
						indent++;
						break;
				}
			}

			// Process the 'add' button if the last			
			im.SameLine();
			if (im.Button($"+###add{GetHashCode()}")) AddNext();

			// Unindent when returning from a subexpression
			if (operatorToken.Operation == DiceOperation.EndSubExpression && next != null) {
				im.Unindent();
				indent--;
			}

			// Draw the next element
			next?.Draw(ref indent);
		}

		private void RemoveSelf() {
			if (previous != null) previous.next = next;
			if (next != null) next.previous = previous;
		}

		private void AddNext() {
			var newNext = new ElementVisualDiceExpression {
				previous = this,
				next = this.next
			};
			if (next != null) next.previous = newNext;
			next = newNext;
		}

		private void AppendTokens(List<DiceToken> tokenList, ref int depth) {
			// Only append the operation first if it is valid to do so
			if (previous != null && previous.valueToken.Operation != DiceOperation.BeginSubExpression) {
				if (operatorToken.Operation == DiceOperation.EndSubExpression) {
					// Only append the end when we are deeper that the top level
					if (depth > 0) tokenList.Add(operatorToken);
					depth--;
				} else tokenList.Add(operatorToken);
			}

			// Only append the value if it is not an end of expression
			if (operatorToken.Operation != DiceOperation.EndSubExpression) {
				tokenList.Add(valueToken);
				// Do special handling if we are beginning a sub-expression
				if (valueToken.Operation == DiceOperation.BeginSubExpression) depth++;
			}

			// Call the next object in the chain
			next?.AppendTokens(tokenList, ref depth);
		}

		public DiceExpression ToExpression() {
			// Make sure we're at the start of the chain
			if (previous != null) return previous.ToExpression();

			// Append tokens from the beginning
			List<DiceToken> tokens = new();
			int depth = 0;
			AppendTokens(tokens, ref depth);

			// Append any missing ')' tokens
			while (depth-- > 0) tokens.Add(new DiceToken(DiceOperation.EndSubExpression));

			return new DiceExpression(tokens);
		}

		public bool TryGetExpression([NotNullWhen(true)] out DiceExpression? expression) {
			try {
				expression = ToExpression();
				return true;
			} catch(Exception) {
				expression = null;
				return false;
			}
		}

	}

	public class WindowDiceTray : UIWindow {

		private static readonly int[] quickDice = new int[] { 4, 6, 8, 10, 12, 20, 100 };
		private static readonly ImGuiTextBuffer actionText = new();

		// Table for the text quick reference
		private static readonly (string, string)[] referenceText = new (string, string)[] {
			("dX", "Rolls a single die of size X"),
			("NdX", "Rolls N dice of size X, summing the results"),
			("!", "'Explodes' the preceding dice roll, rerolling while the maximum is rolled and summing"),
			("+,-", "Adds/subtracts two expressions"),
			("(,)", "Evaluates everything within the brackets as its own expression"),
			("^", "Takes the maximum of two expressions"),
			("_", "Takes the minimum of two expressions"),
			("${X}", "Uses the variable X from the active character")
		};

		private DiceExpression? actionExpression = null;
		private string? errorText = null;

		private ElementVisualDiceExpression visualExpression = new();

		public WindowDiceTray() : base("Dice Tray") { }

		protected override void DrawInternal() {
			var im = GImGui.Instance;

			{ // Draw quick action table
				im.Text("Quick Actions"u8);
				im.Separator();
				const int NumVariants = 10;
				if (im.BeginTable("quickDice", NumVariants + 1, ImGuiTableFlags.SizingFixedFit)) {
					im.TableNextRow();
					foreach (int die in quickDice) {
						im.TableNextColumn();
						im.Text($"d{die}");
						for (int i = 1; i <= NumVariants; i++) {
							im.TableNextColumn();
							var str = $"{i}d{die}";
							if (im.SmallButton(str)) Roll(new DiceExpression(str).Evaluate());
						}
					}
					im.EndTable();
				}
			}
			{ // Draw custom actions
				im.Text("Custom Actions"u8);
				im.Separator();
			}
			{ // Draw action builder
				im.Text("Action Builder"u8);
				im.Separator();
				if (im.BeginTabBar("###diceBuilderTabs")) {
					// Draw the graphical action builder
					if (im.BeginTabItem("Graphical", true)) {
						int indent = 0;
						visualExpression.Draw(ref indent);
						while (indent-- > 0) im.Unindent();
						im.Separator();
						if (im.Button("Roll"u8) && visualExpression.TryGetExpression(out DiceExpression? expr)) Roll(expr.Evaluate());
						im.SameLine();
						if (im.Button("Save"u8)) ;
						im.SameLine();
						if (UI.ColoredButton("Clear", new Vector4(0.8f, 0, 0, 1)))
							visualExpression = new ElementVisualDiceExpression();
						im.EndTabItem();
					}
					// Draw the advanced action builder
					if (im.BeginTabItem("Advanced", true)) {
						// Draw the expression text box and controls
						if (im.InputText("##diceTrayAction", actionText, ImGuiInputTextFlags.CallbackResize)) {
							try {
								actionExpression = new DiceExpression(actionText);
								errorText = null;
							} catch (FormatException ex) {
								errorText = ex.Message;
							}
						}
						if (errorText != null) im.TextColored(new Vector4(1, 0, 0, 1), errorText);
						if (im.Button("Roll"u8) && actionExpression != null) Roll(actionExpression.Evaluate());
						im.SameLine();
						if (im.Button("Save"u8)) ;
						im.SameLine();
						if (UI.ColoredButton("Clear"u8, new Vector4(0.8f, 0, 0, 1))) {
							actionText.Clear();
							actionExpression = null;
							errorText = null;
						}
						// Draw the quick reference
						im.Text("Action Quick Reference:"u8);
						if (im.BeginTable("actionReference", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Border | ImGuiTableFlags.RowBg)) {
							foreach (var refInfo in referenceText) {
								im.TableNextRow();
								im.TableNextColumn();
								im.Text(refInfo.Item1);
								im.TableNextColumn();
								im.Text(refInfo.Item2);
							}
							im.EndTable();
						}
						im.Text("Expressions are evaluated from left to right (eg. '1^3_2' evaluates to 2).");
						im.EndTabItem();
					}
					im.EndTabBar();
				}
			}
		}

		private async void Roll(Task<DiceResult> deferredResult) {
			DiceResult result = await deferredResult;
			await UI.ShowMessageDialog(result.DisplayText);
		}

	}

}
