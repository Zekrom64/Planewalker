using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract.Core.Graphics;
using Tesseract.ImGui;
using Tesseract.ImGui.OpenGL;
using Tesseract.OpenGL;
using Tesseract.SDL;
using Tesseract.SDL.Services;
using Tesseract.Core.Services;
using Tesseract.ImGui.Core;
using Tesseract.Core.Input;
using Tesseract.CLI.ImGui;
using System.Numerics;
using Tesseract.Core.Resource;
using Tesseract.Core.Utilities;
using Tesseract.CLI.ImGui.Addon;

namespace Planewalker {

	public class Display : IDisposable {

		public IInputSystem InputSystem { get; } = GlobalServices.GetGlobalService(InputServices.InputSystem)!;

		public IWindowSystem WindowSystem { get; } = GlobalServices.GetGlobalService(GraphicsServices.WindowSystem)!;

		public IWindow Window { get; }

		public IGLContext GLContext { get; }

		public GL GL { get; }

		public GL45 GL45 { get; }

		public Display() {
			Window = WindowSystem.CreateWindow("Planewalker", 1200, 900, new WindowAttributeList() {
				{ WindowAttributes.Resizable, true },
				{ WindowAttributes.Maximized, true },

				{ GLWindowAttributes.OpenGLWindow, true },
				{ GLWindowAttributes.ContextVersionMajor, 4 },
				{ GLWindowAttributes.ContextVersionMinor, 5 }
			});
			Window.StartTextInput();

			GLContext = Window.GetService(GLServices.GLContextProvider)!.CreateContext();
			GLContext.SetGLSwapInterval(1);
			GL = new GL(GLContext);
			GL45 = GL.GL45!;

			GImGui.Instance = new ImGuiCLI();
			GImGui.CurrentContext = GImGui.CreateContext();
			ImGuiCoreInput.Init(InputSystem, Window, WindowSystem);
			ImGuiOpenGL45.Init(GL);


			var fonts = GImGui.IO.Fonts;
			const float fontSize = 18;
			using Stream notoSans = new ResourceLocation("Fonts/NotoSans-SemiBold.ttf").OpenStream();
			fonts.AddFontFromMemoryTTF(notoSans.ReadFully(), fontSize);
			using Stream fontAwesome = new ResourceLocation("Fonts/Font Awesome 6 Free-Solid-900.ttf").OpenStream();
			fonts.AddFontFromMemoryTTF(fontAwesome.ReadFully(), fontSize, new ImFontConfig() {
				MergeMode = true,
				GlyphRanges = new List<(char, char)>() {
					('\u0020', '\u00F7'),
					('\u2013', '\u2B50'),
					('\uE005', '\uE5B4'),
					('\uF000', '\uF8FF')
				}
			});
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
			ImGuiOpenGL45.Shutdown();
			ImGuiCoreInput.Shutdown();
			Window.Dispose();
		}

		public void PollInput() {
			ImGuiCoreInput.NewFrame();
			ImGuiOpenGL45.NewFrame();
			GImGui.NewFrame();
		}

		public void Draw() {
			GL45.ColorClearValue = new Vector4(0, 0, 0, 1);
			GL45.Clear(GLBufferMask.Color);

			GImGui.Render();
			ImGuiOpenGL45.RenderDrawData(GImGui.GetDrawData());

			GLContext.SwapGLBuffers();
		}

	}

}
