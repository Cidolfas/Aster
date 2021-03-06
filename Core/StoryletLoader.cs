#define DEBUG_LOAD

using System.Collections.Generic;
using System.IO;
using System.Net;
using System;

namespace Azalea.Core
{
    public class StoryletLoader
    {
		Storylet currentStorylet = null;
		Dictionary<string, Storylet> library = null;
		bool inTextBlock = false;
		bool inLink = false;

        public void LoadTextFile(string path, Dictionary<string, Storylet> l)
		{
			library = l;
			CloseCurrent();

#if DEBUG_LOAD
			Data.Log("Loading storylet file at " + path);
#endif

			var f = File.OpenText(path);
			string line = f.ReadLine();
			
			while (AddLine(line))
			{
				line = f.ReadLine();
			}

			CloseCurrent();
		}

		public void LoadGDoc(string docID, Dictionary<string, Storylet> l)
		{
			string url = string.Format("https://docs.google.com/document/d/{0}/export?format=txt", docID);

			WebClient wc = new WebClient();
			string contents = wc.DownloadString(url);
			wc.Dispose();
			string[] lines = contents.Split(new string[] {"\r\n", "\n\r", "\n", "\r"}, StringSplitOptions.None);
			LoadEnumerator(((IEnumerable<string>)lines).GetEnumerator(), l);
		}

		public void LoadEnumerator(IEnumerator<string> e, Dictionary<string, Storylet> l)
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
				inTextBlock = false;
			}
			else if (currentStorylet != null)
			{
				if (line == "-")
				{
					inTextBlock = false;
				}
				else if (line.StartsWith("-Body"))
				{
					inTextBlock = true;
					inLink = false;
				}
				else if (line.StartsWith("-Link"))
				{
					inTextBlock = true;
					inLink = true;
				}
				else if (inTextBlock)
				{
					AddTextLine(line, inLink);
				}
				else if (line == "PassThrough")
				{
					currentStorylet.Type = Storylet.NodeType.PassThrough;
				}
				else if (line == "Chance")
				{
					currentStorylet.Type = Storylet.NodeType.Chance;
				}
				else if (line == "Location")
				{
					currentStorylet.IsLocation = true;
				}
				else if (line.StartsWith("Test "))
				{
					currentStorylet.Type = Storylet.NodeType.Test;
					AddTest(line);
				}
				else if (line.StartsWith("Title: "))
				{
					currentStorylet.Title = line.Replace("Title: ", "");
					if (string.IsNullOrEmpty(currentStorylet.LinkTitle) || currentStorylet.LinkTitle == "NoTitle")
					{
						currentStorylet.LinkTitle = currentStorylet.Title;
					}
				}
				else if (line.StartsWith("LinkTitle: "))
				{
					currentStorylet.LinkTitle = line.Replace("LinkTitle: ", "");
				}
				else if (line.StartsWith("Req: "))
				{
					AddRequirement(line);
				}
				else if (line.StartsWith("Q: "))
				{
					var op = new Inventory.Operation();
					if (op.FromLine(line))
						currentStorylet.Operations.Add(op);
				}
				else if (line.StartsWith("Move "))
				{
					AddMove(line);
				}
				else if (line.StartsWith("@"))
				{
					AddLink(line);
				}
			}

			return true;
		}

		protected void CloseCurrent()
		{
			inTextBlock = false;
			inLink = false;

			if (currentStorylet == null)
				return;

			currentStorylet.Body.Trim();
			if (!library.ContainsKey(currentStorylet.Name))
			{
#if DEBUG_LOAD
				Data.Log("Added Storylet " + currentStorylet.Name + " " + currentStorylet.Title);
#endif
				library.Add(currentStorylet.Name, currentStorylet);
			}
			currentStorylet = null;
		}

		protected void CreateNew(string line)
		{
			CloseCurrent();

			string[] chunks = line.Split(' ');
			if (chunks.Length < 2)
			{
				Data.Log("ERROR: Trying to create storylet without a name!");
				return;
			}

			currentStorylet = new Storylet();
			currentStorylet.Name = chunks[1];
		}

		protected void AddTextLine(string line, bool inLink)
		{
			if (currentStorylet == null)
				return;

			line += "\n";

			if (inLink)
			{
				currentStorylet.LinkText += line;
			}
			else
			{
				currentStorylet.Body += line;
			}
		}

		protected void AddTest(string line)
		{
			if (currentStorylet == null)
				return;

			var test = new Storylet.Test();
			if (test.FromLine(line))
				currentStorylet.Tests.Add(test);
		}

		protected void AddRequirement(string line)
		{
			if (currentStorylet == null)
				return;

			var req = new Storylet.Requirement();
			if (req.FromLine(line))
				currentStorylet.Requirements.Add(req);
		}

		protected void AddLink(string line)
		{
			if (currentStorylet == null)
				return;

			var link = new Storylet.Link();
			if (link.FromLine(line, currentStorylet.Type))
				currentStorylet.Links.Add(link);
		}

		protected void AddMove(string line)
		{
			if (currentStorylet == null)
				return;

			string[] chunks = line.Split(' ');
			if (chunks.Length < 2)
			{
				Data.Log("ERROR: Trying to create move op without destination!");
				return;
			}

			currentStorylet.MoveLoc = chunks[1];
		}
    }
}