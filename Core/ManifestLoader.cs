using System.IO;

namespace Azalea.Core
{
    public static class ManifestLoader
    {
        public static bool LoadingStorylets = true;

        public static void LoadManifest(string path)
        {
            LoadingStorylets = true;

            var f = File.OpenText(path);
			string line = f.ReadLine();
			
			while (AddLine(line))
			{
				line = f.ReadLine();
			}
        }

        public static bool AddLine(string line)
        {
            if (line == null)
                return false;

            if (line.StartsWith("#"))
            {
                return true;
            }
            else if (line == "Storylets")
            {
                LoadingStorylets = true;
            }
            else if (line == "Qualities")
            {
                LoadingStorylets = false;
            }
            else if (line.StartsWith("gdoc: "))
            {
                string url = GetPath(line);
                if (url != null)
                {
                    if (LoadingStorylets)
                    {
                        var loader = new StoryletLoader();
                        loader.LoadGDoc(url, Data.Storylets);
                    }
                    else
                    {
                        var loader = new QualityLoader();
                        loader.LoadGDoc(url, Data.Qualities);
                    }
                }
            }
            else if (line.StartsWith("file: "))
            {
                string path = GetPath(line);
                if (path != null)
                {
                    if (LoadingStorylets)
                    {
                        var loader = new StoryletLoader();
                        loader.LoadTextFile(path, Data.Storylets);
                    }
                    else
                    {
                        var loader = new QualityLoader();
                        loader.LoadTextFile(path, Data.Qualities);
                    }
                }
            }
            // else if (line.StartsWith("dir: "))
            // {

            // }

            return true;
        }

        public static string GetPath(string line)
        {
            string[] split = line.Split(' ');

            if (split.Length < 2)
                return null;

            return split[1];
        }
    }
}