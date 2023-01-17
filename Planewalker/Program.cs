
using Planewalker.Content;
using Tesseract.Core.Native;
using Tesseract.Core.Resource;
using Tesseract.SDL;
using Tesseract.SDL.Services;

namespace Planewalker {
	
	public static class Program {

		public static Display Display { get; private set; } = null!;

		private static void Preinit() {
			ResourceDomain.Default = new AssemblyResourceDomain("planewalker", typeof(Program).Assembly) { PathPrefix = "Planewalker/Assets/" };
			LibraryManager.EmbeddedLibraryDomain = ResourceDomain.Default;

			SDLServices.Register();
			SDLGLServices.Register();
		}

		private static void Init() {
			SDL2.Init(SDLSubsystems.Video | SDLSubsystems.Events);
			Display = new Display();
		}

		private static void Shutdown() {
			SDL2.Quit();
		}

		static Program() {
			Preinit();
		}

		public static void Main(string[] args) {
			Init();

			while (!Display.Window.Closing) {
				Display.PollInput();
				UI.Process();
				Display.Draw();
			}
			
			Shutdown();
		}

	}

}
