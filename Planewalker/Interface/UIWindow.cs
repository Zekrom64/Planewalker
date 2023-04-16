using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Tesseract.Core.Numerics;
using Tesseract.ImGui;

namespace Planewalker.Interface {

	public class UIWindow : IDisposable {

		public string Name { get; }

		private string? displayName;
		public string DisplayName {
			get => displayName ?? Name;
			set => displayName = value;
		}

		public ImGuiWindowFlags Flags { get; set; } = ImGuiWindowFlags.None;

		public UIWindow(string name) {
			Name = name;
			UI.Windows.Add(this);
		}

		private bool isAlwaysOpen = false;
		public bool IsAlwaysOpen {
			get => isAlwaysOpen;
			set {
				isAlwaysOpen = value;
				if (value) isOpen = true;
			}
		}

		private bool isOpen;
		public bool IsOpen {
			get => IsAlwaysOpen || isOpen;
			set {
				if (!IsAlwaysOpen) isOpen = value;
			}
		}

		public Vector2 MinSize { get; set; } = new Vector2(100, 100);

		public void Show() => IsOpen = true;

		public void Hide() => IsOpen = false;

		public void Draw() {
			if (IsOpen) {
				var flags = Flags;
				string name = displayName != null ? $"{displayName}###{Name}" : Name;
				if (IsAlwaysOpen) GImGui.Begin(name, true, Flags);
				else GImGui.Begin(name, ref isOpen, flags);
				DrawInternal();
				GImGui.End();
			}
		}

		protected virtual void DrawInternal() { }

		public virtual void Dispose() {
			GC.SuppressFinalize(this);
			UI.Windows.Remove(this);
		}
	}

}
