using System;
using System.Windows.Forms;

namespace MarketQADataProcessorApp
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			//Startup.UpdatePathEnvironmentVariable();
			//ToolkitStartup.StandaloneAppToolkitSettingsInit();

			Application.Run(new formMain());
		}
	}
}
