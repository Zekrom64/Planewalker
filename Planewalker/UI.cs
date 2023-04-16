using Planewalker.Content;
using Planewalker.Interface;
using Planewalker.Interface.Items;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Tesseract.Core;
using Tesseract.ImGui;
using Tesseract.Core.Numerics;
using Tesseract.CLI.ImGui.Addon;

namespace Planewalker {

	public static class UI {

		internal static readonly List<UIWindow> Windows = new();
		internal static readonly List<UIDialog> DialogQueue = new();

		private static float menuBarHeight;
		public static Rectf WindowContentArea { get; private set; }

		private static bool showImGuiAbout = false;
		private static bool showImGuiMetrics = false;

		private static void ProcessMainMenuBar() {
			if (GImGui.BeginMainMenuBar()) {
				if (GImGui.BeginMenu("Tools")) {
					if (GImGui.MenuItem("Dice Tray")) DiceTray.Show();
					GImGui.Separator();
					if (GImGui.MenuItem("Advanced Dice Settings")) AdvancedDiceSettings.Show();
					GImGui.Separator();
					if (GImGui.MenuItem("About ImGui")) showImGuiAbout = true;
					if (GImGui.MenuItem("ImGui Metrics")) showImGuiMetrics = true;
					GImGui.EndMenu();
				}
				menuBarHeight = GImGui.WindowHeight;
				GImGui.EndMainMenuBar();
			}
		}

		public static WindowDiceTray DiceTray { get; } = new() { IsOpen = true };
		public static WindowAdvancedDiceSettings AdvancedDiceSettings { get; } = new();

		public static void Process() {
			ProcessMainMenuBar();
			var areaSize = GImGui.IO.DisplaySize;
			areaSize.Y -= menuBarHeight;
			WindowContentArea = new Rectf() { Position = new Vector2(0, menuBarHeight), Size = areaSize };

			if (showImGuiAbout) GImGui.ShowAboutWindow(ref showImGuiAbout);
			if (showImGuiMetrics) GImGui.ShowMetricsWindow(ref showImGuiMetrics);

			foreach (var window in Windows) window.Draw();

			if (DialogQueue.Count > 0) {
				var dialog = DialogQueue[0];
				dialog.Draw();
				if (dialog.IsCompleted) DialogQueue.RemoveAt(0);
			}
		}

		/// <summary>
		/// Opens the given string URL using the system's default browser.
		/// </summary>
		/// <param name="url">The URL to open</param>
		public static void OpenInBrowser(string url) {
			switch(Platform.CurrentPlatformType) {
				case PlatformType.Windows:
					System.Diagnostics.Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
					break;
				case PlatformType.Linux:
					System.Diagnostics.Process.Start("xdg-open", url);
					break;
				case PlatformType.MacOSX:
					System.Diagnostics.Process.Start("open", url);
					break;
			}
		}

		public static async Task<string> ShowInputDialog(string prompt) => await new DialogString("stringInput", prompt).Show(UIDialogMode.Ok);

		public static async Task ShowMessageDialog(string prompt) => await new UIDialog("message", prompt).Show(UIDialogMode.Ok);

		public static bool ColoredButton(string label, Vector4 color, Vector2 size = default) {
			var im = GImGui.Instance;
			im.PushStyleColor(ImGuiCol.Button, color);
			im.PushStyleColor(ImGuiCol.Button, (color + new Vector4(0.2f)).Min(Vector4.One));
			bool ret = im.Button(label, size);
			im.PopStyleColor(2);
			return ret;
		}

		public static bool ColoredButton(ReadOnlySpan<byte> label, Vector4 color, Vector2 size = default) {
			var im = GImGui.Instance;
			im.PushStyleColor(ImGuiCol.Button, color);
			im.PushStyleColor(ImGuiCol.Button, (color + new Vector4(0.2f)).Min(Vector4.One));
			bool ret = im.Button(label, size);
			im.PopStyleColor(2);
			return ret;
		}

	}

}
