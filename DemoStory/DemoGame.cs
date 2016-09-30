using Azalea.Core;

namespace Azalea.Demo
{
    public class DemoGame : Game
    {
        public override void Init()
        {
            base.Init();

            Data.LoadStoryletFile("DemoStory/storylets.txt");
			Data.LoadQualitiesFile("DemoStory/qualities.txt");

			JumpToStorylet(Data.GetStorylet("NewGame"));
        }
    }
}