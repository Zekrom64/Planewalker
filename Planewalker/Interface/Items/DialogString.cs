using Tesseract.ImGui;

namespace Planewalker.Interface.Items {

	public class DialogString : UIDialog {

		private readonly ImGuiTextBuffer textBuffer = new();

		public DialogString(string name, string prompt) : base(name, prompt) { }

		protected override void DrawInternal() {
			GImGui.InputText("###text", textBuffer);
		}

		public new async Task<string> Show(UIDialogMode mode = UIDialogMode.Ok) {
			await base.Show(mode);
			return textBuffer;
		}

	}

	public class DialogString<TState> : UIDialog {

		public TState State { get; set; } = default!;

		public Func<TState, string, TState>? OnModify { get; set; } = null;

		public Action<TState>? OnDraw { get; set; } = null;

		private readonly ImGuiTextBuffer textBuffer = new();

		public DialogString(string name, string prompt) : base(name, prompt) { }

		protected override void DrawInternal() {
			if (GImGui.InputText("###text", textBuffer, ImGuiInputTextFlags.CallbackResize)) {
				if (OnModify != null) State = OnModify.Invoke(State, textBuffer);
			}
			OnDraw?.Invoke(State);
		}

		public new async Task<string> Show(UIDialogMode mode = UIDialogMode.Ok) {
			await base.Show(mode);
			return textBuffer;
		}

	}

}
