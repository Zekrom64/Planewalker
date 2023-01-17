using Planewalker.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Tesseract.ImGui;

namespace Planewalker {

	public static class UI {

		private static bool showImGuiAbout = false;
		private static bool showImGuiMetrics = false;

		private static void ProcessMainMenuBar() {
			if (GImGui.BeginMainMenuBar()) {
				if (GImGui.BeginMenu("Tools")) {
					if (GImGui.MenuItem("Dice Tray")) showDiceTray = true;
					GImGui.Separator();
					if (GImGui.MenuItem("Advanced Dice Settings")) showAdvancedDiceSettings = true;
					GImGui.Separator();
					if (GImGui.MenuItem("About ImGui")) showImGuiAbout = true;
					if (GImGui.MenuItem("ImGui Metrics")) showImGuiMetrics = true;
					GImGui.EndMenu();
				}
				GImGui.EndMainMenuBar();
			}
		}

		private static bool showDiceTray = true;
		private static readonly int[] diceTrayQuickDice = new int[] { 4, 6, 8, 10, 12, 20 };
		private static readonly ImGuiTextBuffer diceTrayActionText = new();

		private static readonly (string, string)[] diceTrayReference = new (string, string)[] {
			("dX", "Rolls a single die of size X"),
			("NdX", "Rolls N dice of size X, adding the results"),
			("+,-", "Adds/subtracts two expressions"),
			("(,)", "Evaluates everything within the brackets as its own expression"),
			("!", "'Explodes' the last expression, rerolling and adding until the maximum is not rolled"),
			("^", "Takes the maximum of two expressions"),
			("_", "Takes the minimum of two expressions")
		};

		private static void ProcessDiceTray() {
			GImGui.Begin("Dice Tray", ref showDiceTray);
			{ // Draw quick action table
				GImGui.Text("Quick Actions");
				GImGui.Separator();
				if (GImGui.BeginTable("quickDice", 7, ImGuiTableFlags.SizingFixedFit)) {
					GImGui.TableNextRow();
					foreach (int die in diceTrayQuickDice) {
						GImGui.TableNextColumn();
						GImGui.Text($"d{die}");
						for (int i = 1; i <= 6; i++) {
							GImGui.TableNextColumn();
							var str = $"{i}d{die}";
							if (GImGui.SmallButton(str)) ;
						}
					}
					GImGui.EndTable();
				}
			}
			{ // Draw custom actions
				GImGui.Text("Custom Actions");
				GImGui.Separator();
			}
			{ // Draw action builder
				GImGui.Text("Action Builder");
				GImGui.Separator();
				GImGui.InputText("##diceTrayAction", diceTrayActionText, ImGuiInputTextFlags.CallbackResize);
				if (GImGui.Button("Roll")) ;
				GImGui.SameLine();
				if (GImGui.Button("Save")) ;
				GImGui.SameLine();
				GImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0, 0, 1));
				GImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(1, 0, 0, 1));
				if (GImGui.Button("Clear")) diceTrayActionText.Clear();
				GImGui.PopStyleColor(2);
				GImGui.Text("Action Quick Reference:");
				if (GImGui.BeginTable("actionReference", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Border | ImGuiTableFlags.RowBg)) {
					foreach(var refInfo in diceTrayReference) {
						GImGui.TableNextRow();
						GImGui.TableNextColumn();
						GImGui.Text(refInfo.Item1);
						GImGui.TableNextColumn();
						GImGui.Text(refInfo.Item2);
					}
					GImGui.EndTable();
				}
				GImGui.Text("Expressions are evaluated from left to right (eg. '1^3_2' evaluates to 2).");
			}
			GImGui.End();
		}

		private static bool showAdvancedDiceSettings = true;

		private static void ProcessAdvancedDiceSettings() {
			GImGui.Begin("Advanced Dice Settings", ref showAdvancedDiceSettings);
			{ // RNG and Entropy
				int entropy = RandomSource.AvailableEntropy;
				Vector4 color = new(1, 1, 0, 1);
				if (entropy < 500) color.Y = 0;
				else if (entropy > 1000) color.X = 0;
				GImGui.TextColored(color, $"Available Entropy: {entropy} bits");
				GImGui.Text("Random Number Generation:");
				GImGui.BeginGroup();
				if (GImGui.RadioButton("Strict", DiceExpression.RandomMode == RandomNumberMode.Strict)) DiceExpression.RandomMode = RandomNumberMode.Strict;
				if (GImGui.IsItemHovered()) GImGui.SetTooltip("Every roll is guarenteed to be made using true random numbers");
				if (GImGui.RadioButton("Relaxed", DiceExpression.RandomMode == RandomNumberMode.Relaxed)) DiceExpression.RandomMode = RandomNumberMode.Relaxed;
				if (GImGui.IsItemHovered()) GImGui.SetTooltip("Rolls may be made using pseudorandom numbers if no true random numbers are available");
				GImGui.EndGroup();
			}
			GImGui.End();
		}

		public static void Process() {
			ProcessMainMenuBar();

			if (showImGuiAbout) GImGui.ShowAboutWindow(ref showImGuiAbout);
			if (showImGuiMetrics) GImGui.ShowMetricsWindow(ref showImGuiMetrics);

			if (showDiceTray) ProcessDiceTray();
			if (showAdvancedDiceSettings) ProcessAdvancedDiceSettings();
		}

	}

}
