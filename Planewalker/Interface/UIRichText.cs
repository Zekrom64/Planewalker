using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Tesseract.ImGui;
using Tesseract.OpenGL;

namespace Planewalker.Interface {

	public readonly record struct UIRichFormat {

		public required int Offset { get; init; }

		public Vector4? Color { get; init; } = null;

		public IImFont? Font { get; init; } = null;

		public Action? LinkAction { get; init; } = null;

		public UIRichFormat() { }

		public UIRichFormat With(UIRichFormat newFormat) {
			return new UIRichFormat() {
				Offset = newFormat.Offset,
				Color = newFormat.Color ?? Color,
				Font = newFormat.Font ?? Font
			};
		}

	}

	public class UIRichText {

		private readonly string text;
		private readonly UIRichFormat[] formatting;


		public UIRichText(string text, params UIRichFormat[] formatting) {
			this.text = text;
			this.formatting = formatting;
		}

		public UIRichText Clone() => new(text, formatting);


		private struct Segment {

			public string Text;

			public Vector2 Offset;

			public IImFont Font;

			public uint Color;

			public Action? LinkAction;

		}

		private float lastWrap = float.NaN;
		private readonly List<Segment> segments = new();

		public void Draw() {
			if (formatting.Length == 0) {
				// Fast rendering for unformatted text
				GImGui.Text(text);
				return;
			} else if (formatting.Length == 1 && formatting[0].Offset == 0) {
				// Fast rendering for simple formatted text

			}

			// Check that we have computed text segments for the correct wrapping
			float wrap = GImGui.ItemRectSize.X;
			if (wrap != lastWrap) {
				// Initial state
				UIRichFormat format = new() {
					Offset = 0,
					Color = GImGui.GetStyleColorVec4(ImGuiCol.Text),
					Font = GImGui.Font
				};
				int start = 0;

				// Iterate each block of formatted text
				for (int i = 0; i <= formatting.Length; i++) {
					bool last = i == formatting.Length;
					int end = last ? text.Length : formatting[i].Offset;
					string subtext = text[start..end];



					start = end;
					if (!last) format = format.With(formatting[i]);
				}
			}
		}

	}

}
