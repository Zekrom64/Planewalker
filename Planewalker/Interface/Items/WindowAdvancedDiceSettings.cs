using Planewalker.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Tesseract.ImGui;

namespace Planewalker.Interface.Items {

	public class WindowAdvancedDiceSettings : UIWindow {

		public WindowAdvancedDiceSettings() : base("Advanced Dice Settings") { }

		protected override void DrawInternal() {
			var im = GImGui.Instance;
			{ // RNG and Entropy
				int entropy = RandomSource.AvailableEntropy;
				Vector4 color = new(1, 1, 0, 1);
				if (entropy < 500) color.Y = 0;
				else if (entropy > 1000) color.X = 0;
				im.TextColored(color, $"Available Entropy: {entropy} bits");
				im.Text("Random Number Generation:"u8);
				im.BeginGroup();
				if (im.RadioButton("Strict"u8, DiceExpression.RandomMode == RandomNumberMode.Strict)) DiceExpression.RandomMode = RandomNumberMode.Strict;
				if (im.IsItemHovered()) im.SetTooltip("Every roll is guarenteed to be made using true random numbers"u8);
				if (im.RadioButton("Relaxed"u8, DiceExpression.RandomMode == RandomNumberMode.Relaxed)) DiceExpression.RandomMode = RandomNumberMode.Relaxed;
				if (im.IsItemHovered()) im.SetTooltip("Rolls may be made using pseudorandom numbers if no true random numbers are available"u8);
				im.EndGroup();
			}
			{ // Current dice stats

			}
		}

	}

}
