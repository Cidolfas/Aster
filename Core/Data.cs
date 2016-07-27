using System.Collections.Generic;
using System;
using System.IO;

namespace Aster.Core
{
	public static class Data
	{
		public static Dictionary<string, Storylet> Storylets = new Dictionary<string, Storylet>();
		public static Dictionary<string, Quality> Qualities = new Dictionary<string, Quality>();
		public static Random GenericRandom = new Random();

		public static Game CurrentGame;

		public static void Log(string s, params object[] arg)
		{
			Console.WriteLine(s, arg);
		}

		public static Game NewGame()
		{
			var g = new Game();

			var result = new ActionResult();
			g.GoToStorylet(Data.GetStorylet("NewGame"), result);

			return g;
		}

		public static Storylet GetStorylet(string name)
		{
			if (Storylets.ContainsKey(name))
				return Storylets[name];

			return null;
		}

		public static Quality GetQuality(string name)
		{
			if (Qualities.ContainsKey(name))
				return Qualities[name];

			return null;
		}

		public static void LoadStoryletFile(string path)
		{
			var f = File.OpenText(path);
			string line;
			bool inTextBlock = false;
			Storylet currentStorylet = null;
			while (true)
			{
				line = f.ReadLine();

				if (line == null)
				{
					// Close out the last storylet
					if (currentStorylet != null)
					{
						currentStorylet.Body.Trim();
						if (!Storylets.ContainsKey(currentStorylet.Name))
						{
							Storylets.Add(currentStorylet.Name, currentStorylet);
						}
					}

					break;
				}

				if (line.StartsWith("!! "))
				{
					// Close out the last storylet
					if (currentStorylet != null)
					{
						currentStorylet.Body.Trim();
						if (!Storylets.ContainsKey(currentStorylet.Name))
						{
							Storylets.Add(currentStorylet.Name, currentStorylet);
						}
					}

					currentStorylet = new Storylet();
					string[] chunks = line.Split(' ');
					currentStorylet.Name = (chunks.Length > 1) ? chunks[1] : "NoName";
					inTextBlock = false;
				}
				else if (currentStorylet != null)
				{
					if (line == "-")
					{
						inTextBlock = false;
					}
					else if (inTextBlock)
					{
						currentStorylet.Body += line + "\n";
					}
					else if (line.StartsWith("-Body"))
					{
						currentStorylet.Body = "";
						inTextBlock = true;
					}
					else if (line == "PassThrough")
					{
						currentStorylet.Type = Storylet.NodeType.PassThrough;
					}
					else if (line == "Chance")
					{
						currentStorylet.Type = Storylet.NodeType.Chance;
					}
					else if (line.StartsWith("Test "))
					{
						currentStorylet.Type = Storylet.NodeType.Test;
						var chunks = line.Split(' ');
						currentStorylet.TestQualityName = chunks[1];
						currentStorylet.TestQualityAmount = int.Parse(chunks[2]);
						if (chunks.Length > 3)
						{
							currentStorylet.TestNarrow == (chunks[3] == "Narrow");
						}
					}
					else if (line.StartsWith("Title: "))
					{
						currentStorylet.Title = line.Replace("Title: ", "");
					}
					else if (line.StartsWith("Req: "))
					{
						var req = new Storylet.Requirement();
						if (req.FromLine(line.Split(' ')))
							currentStorylet.Requirements.Add(req);
					}
					else if (line.StartsWith("Q: "))
					{
						var op = new Inventory.Operation();
						if (op.FromLine(line))
							currentStorylet.EntryOperations.Add(op);
					}
					else if (line.StartsWith("Onwards: "))
					{
						currentStorylet.OnwardsName = line.Replace("Onwards: ", "").Remove(0, 1);
					}
					else if (line.StartsWith("@"))
					{
						switch (currentStorylet.Type)
						{
							case Storylet.NodeType.Normal:
								var chunks = line.Split(new []{' '}, 2);
								currentStorylet.Links.Add(new Storylet.Link(chunks[0].Remove(0, 1), (chunks.Length > 1) ? chunks[1] : chunks[0]));
								break;

							case Storylet.NodeType.Chance:
								chunks = line.Split(' ');
								int weight;
								if (!int.TryParse(chunks[1], out weight))
									weight = 1;
								currentStorylet.Links.Add(new Storylet.Link(chunks[0].Remove(0, 1), weight));
								break;

							case Storylet.NodeType.PassThrough:
								currentStorylet.Links.Add(new Storylet.Link(line.Remove(0, 1), null));
								break;

							case Storylet.NodeType.Test:
								chunks = line.Split(' ');
								weight;
								if (!int.TryParse(chunks[2], out weight))
									weight = 1;
								currentStorylet.Links.Add(new Storylet.Link(chunks[0].Remove(0, 1), chunks[1], weight));
								break;
						}
					}
				}
			}
		}

		public static void LoadQualitiesFile(string path)
		{
			var f = File.OpenText(path);
			string line;
			Quality currentQuality = null;
			while (true)
			{
				line = f.ReadLine();

				if (line == null)
				{
					if (currentQuality != null)
					{
						if (!Qualities.ContainsKey(currentQuality.Name))
						{
							Qualities.Add(currentQuality.Name, currentQuality);
						}
					}

					break;
				}

				if (currentQuality != null)
				{
					if (line.StartsWith("!!"))
					{
						if (currentQuality != null)
						{
							if (!Qualities.ContainsKey(currentQuality.Name))
							{
								Qualities.Add(currentQuality.Name, currentQuality);
							}
						}

						currentQuality = new Quality();

						string[] chunks = line.Split(' ');
						currentQuality.Name = (chunks.Length > 1) ? chunks[1] : "NoName";
						currentQuality.Title = currentQuality.Name;
					}
					else if (line.StartsWith("Title: "))
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
					else
					{
						currentQuality.Description = line;
					}
				}
			}
		}
	}
}