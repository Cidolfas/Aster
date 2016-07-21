using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Nancy.Owin;

namespace Aster
{
	public class Startup
	{
		public void Configure(IApplicationBuilder app)
		{
			app.UseOwin(x => x.UseNancy());
		}

		// Entry point for the application.
		public static void Main()
		{
			Core.Data.LoadStoryletFile("DemoStory/storylets.txt");
			Core.Data.LoadQualitiesFile("DemoStory/qualities.txt");

			Core.Data.CurrentGame = Core.Data.NewGame();

            new WebHostBuilder().UseKestrel().UseStartup<Startup>().Build().Run();
		}
    }
}
