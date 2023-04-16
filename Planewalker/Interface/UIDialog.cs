using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Tesseract.ImGui;

namespace Planewalker.Interface {

	[Flags]
	public enum UIDialogMode {
		Ok = 0x01,
		Cancel = 0x02,
		Yes = 0x04,
		No = 0x08,

		Closeable = 0x80,

		OkCancel = Ok | Cancel
	}

	public enum UIDialogResult {
		Yes,
		No,
		Cancel
	}

	public class UIDialog {

		public string Name { get; }

		public string Prompt { get; }

		internal readonly TaskCompletionSource<UIDialogResult> completion = new();

		public UIDialog(string name, string prompt) {
			Name = name;
			Prompt = prompt;
		}

		private UIDialogMode mode;
		private UIDialogResult? result;

		public bool IsCompleted => completion.Task.IsCompleted;

		public UIDialogResult Result => result ?? throw new InvalidOperationException("Cannot get result before the dialog has completed");

		public Task<UIDialogResult> Show(UIDialogMode mode) {
			UI.DialogQueue.Add(this);
			this.mode = mode;
			return completion.Task;
		}

		private bool CheckMode(UIDialogMode mode) => (this.mode & mode) != 0;

		private void SetResult(UIDialogResult result) {
			this.result = result;
			completion.SetResult(result);
		}

		public void Draw() {
			GImGui.SetNextWindowPos(GImGui.MainViewport.Center, ImGuiCond.Appearing, new Vector2(0.5f));
			bool open = true;
			GImGui.OpenPopup(Name);
			var flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.Modal | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.AlwaysAutoResize;
			if (!CheckMode(UIDialogMode.Closeable)) flags |= ImGuiWindowFlags.NoDecoration;
			GImGui.BeginPopupModal(Name, ref open, flags);
			if (!open) completion.SetResult(UIDialogResult.Cancel);

			GImGui.Text(Prompt);

			DrawInternal();

			GImGui.NewLine();

			bool hasPrevButton = false;
			void MakeSameLine() {
				if (hasPrevButton) GImGui.SameLine();
				hasPrevButton = true;
			}

			void MakeButton(UIDialogMode mode, UIDialogResult result, Vector4? color = null) {
				if (CheckMode(mode)) {
					MakeSameLine();
					if (color != null) {
						if (UI.ColoredButton(mode.ToString(), color.Value)) SetResult(result);
					} else {
						if (GImGui.Button(mode.ToString())) SetResult(result);
					}
				}
			}

			MakeButton(UIDialogMode.Ok, UIDialogResult.Yes);
			MakeButton(UIDialogMode.Cancel, UIDialogResult.Cancel);
			MakeButton(UIDialogMode.Yes, UIDialogResult.Yes, new Vector4(0, 1, 0, 1));
			MakeButton(UIDialogMode.No, UIDialogResult.No, new Vector4(1, 0, 0, 1));

			GImGui.EndPopup();
		}

		protected virtual void DrawInternal() { }

	}

}
