using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Nancy.Owin;

namespace Azalea
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
			Core.Data.CurrentGame = new Demo.DemoGame();
			Core.Data.CurrentGame.Init();

            new WebHostBuilder().UseKestrel().UseStartup<Startup>().Build().Run();
		}
    }
}
