using Azalea.Core;

namespace Azalea.Demo
{
    public class DemoGame : Game
    {
        public override void Init()
        {
            base.Init();

            // Data.LoadStoryletFile("DemoStory/storylets.txt");
			// Data.LoadQualitiesFile("DemoStory/qualities.txt");

            ManifestLoader.LoadManifest("manifest.txt");

			JumpToStorylet(Data.GetStorylet("NewGame"));
        }
    }
}