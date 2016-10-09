#define DEBUG_LOAD

using System.Collections.Generic;
using System.IO;
using System.Net;
using System;

namespace Azalea.Core
{
    public class QualityLoader
    {
		Quality currentQuality = null;
		Dictionary<string, Quality> library = null;

        public void LoadTextFile(string path, Dictionary<string, Quality> l)
		{
			library = l;
            CloseCurrent();

#if DEBUG_LOAD
			Data.Log("Loading quality file at " + path);
#endif

			var f = File.OpenText(path);
			string line = f.ReadLine();
			
			while (AddLine(line))
			{
				line = f.ReadLine();
			}

            CloseCurrent();
		}

        public void LoadGDoc(string docID, Dictionary<string, Quality> l)
		{
            string url = string.Format("https://docs.google.com/document/d/{0}/export?format=txt", docID);

			WebClient wc = new WebClient();
			string contents = wc.DownloadString(url);
			wc.Dispose();
			string[] lines = contents.Split(new string[] {"\r\n", "\n\r", "\n", "\r"}, StringSplitOptions.None);
			LoadEnumerator(((IEnumerable<string>)lines).GetEnumerator(), l);
		}

		public void LoadEnumerator(IEnumerator<string> e, Dictionary<string, Quality> l)
		{
			library = l;
			CloseCurrent();
			
			while(e.MoveNext() && AddLine(e.Current))
			{
			}

            CloseCurrent();
		}

        protected bool AddLine(string line)
        {
            if (line == null)
            {
                // Close out the last storylet
                CloseCurrent();
                return false;
            }

            if (line.StartsWith("!! "))
            {
                CreateNew(line);
            }
            else if (currentQuality != null)
            {
                if (line.StartsWith("Title: "))
                {
                    currentQuality.Title = line.Replace("Title: ", "");
                }
                else if (line.StartsWith("Tags: "))
                {
                    string[] chunks = line.Replace("Tags: ", "").Split(' ');
                    if (chunks.Length > 0 && !string.IsNullOrEmpty(chunks[0]))
                        currentQuality.Tags.AddRange(chunks);
                }
                else if (line.StartsWith("Storylet: "))
                {
                    string[] chunks = line.Split(' ');
                    currentQuality.StoryletName = (chunks.Length > 1) ? chunks[1] : null;
                }
                else if (line.StartsWith("Curve: "))
                {
                    string[] chunks = line.Split(' ');
                    currentQuality.Curve = (chunks.Length > 1) ? chunks[1] : null;
#if DEBUG_LOAD
                    if (currentQuality.Curve == null || Data.LevelCurves.ContainsKey(currentQuality.Curve))
                    {
                        Data.Log("Could not find a quality curve for line <{0}>", line);
                    }
#endif
                }
                else if (!string.IsNullOrEmpty(line))
                {
                    currentQuality.Description = line;
                }
            }

            return true;
        }

		protected void CloseCurrent()
		{
			if (currentQuality == null)
				return;

			if (!library.ContainsKey(currentQuality.Name))
			{
#if DEBUG_LOAD
				Data.Log("Added Quality " + currentQuality.Name + " " + currentQuality.Title);
#endif
				library.Add(currentQuality.Name, currentQuality);
			}
			currentQuality = null;
		}

		protected void CreateNew(string line)
		{
			CloseCurrent();

			string[] chunks = line.Split(' ');
			if (chunks.Length < 2)
			{
				Data.Log("ERROR: Trying to create quality without a name!");
				return;
			}

			currentQuality = new Quality();
			currentQuality.Name = chunks[1];
		}
    }
}