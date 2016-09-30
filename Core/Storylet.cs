using System;
using System.Collections.Generic;

namespace Azalea.Core
{
	public class Storylet
	{
		public class Link
		{
			public enum TestResult { None, Success, Failure }

			public string Special = null;
			public string StoryletName;
			public int Weight = 1;
			public TestResult Test = TestResult.None;

			public static Link GetOnwards(string sName)
			{
				var l = new Link();
				l.Special = "Onwards";
				l.StoryletName = sName;
				return l;
			}

			public static Link GetReturn(string sName)
			{
				var l = new Link();
				l.Special = "Return";
				l.StoryletName = sName;
				return l;
			}

			public bool FromLine(string line, NodeType type)
			{
				var chunks = line.Split(' ');
				int idx = 0;

				StoryletName = chunks[idx++].Remove(0, 1);

				while (++idx < chunks.Length)
				{
					if (chunks[idx] == "Success")
					{
						Test = TestResult.Success;
					}
					else if (chunks[idx] == "Failure")
					{
						Test = TestResult.Failure;
					}
					else
					{
						int w;
						if (int.TryParse(chunks[idx], out w))
						{
							Weight = w;
						}
					}
				}

				if (type == NodeType.Test && Test == TestResult.None)
					return false;

				return true;
			}
		}

		public class Requirement
		{
			public enum ReqType { None, Equals, LessThan, AtLeast, Has, HasNo, Range }

			public ReqType Req;
			public string QualityName;
			public int Value;
			public int ValueTwo;
			public bool NoShow; // Controls if we should show this storylet or not based on this req

			public bool FromLine(string line)
			{
				string[] chunks = line.Split(' ');
				int idx = 0;

				if (chunks.Length < 3)
					return false;

				if (chunks[idx++] != "Req:")
					return false;

				QualityName = chunks[idx++];

				if (!Enum.TryParse<ReqType>(chunks[idx++], true, out Req))
					return false;

				switch (Req)
				{
					case ReqType.None:
						return false;

					case ReqType.Has:
					case ReqType.HasNo:
						break;

					case ReqType.Equals:
					case ReqType.LessThan:
					case ReqType.AtLeast:
						if (chunks.Length < idx + 1)
							return false;

						if (!int.TryParse(chunks[idx++], out Value))
							return false;

						break;

					case ReqType.Range:
						if (chunks.Length < idx + 2)
							return false;

						if (!int.TryParse(chunks[idx++], out Value) || !int.TryParse(chunks[idx++], out ValueTwo))
							return false;

						break;

					default:
						return false;
				}

				while (idx < chunks.Length)
				{
					string cmd = chunks[idx++];

					switch (cmd)
					{
						case "NoShow":
							NoShow = true;
							break;

						default:
							break;
					}
				}

				return true;
			}

			public bool IsMet(Inventory inv)
			{
				Quality qual = Data.GetQuality(QualityName);
				int quantity = inv.GetCount(QualityName);
				if (qual != null)
					quantity = qual.GetLevel(quantity);

				switch (Req)
				{
					case ReqType.None:
						return false;

					case ReqType.Equals:
						return quantity == Value;

					case ReqType.LessThan:
						return quantity < Value;

					case ReqType.AtLeast:
						return quantity >= Value;

					case ReqType.Has:
						return quantity > 0;

					case ReqType.HasNo:
						return quantity == 0;

					case ReqType.Range:
						return (quantity >= Value) && (quantity <= ValueTwo);

					default:
						return false;
				}
			}
		}

		public class Test
		{
			public string QualityName;
			public int Value = 0;
			public int ValueTwo = -1;
			public string TestName;
			public bool Invert;

			public bool FromLine(string line)
			{
				string[] chunks = line.Split(' ');
				int idx = 0;

				if (chunks.Length < 3)
					return false;

				if (chunks[idx++] != "Test")
					return false;

				QualityName = chunks[idx++];
				if (!int.TryParse(chunks[idx++], out Value))
				{
					return false;
				}

				if (chunks.Length > 3 && !int.TryParse(chunks[idx++], out ValueTwo))
				{
					idx--;
				}

				while (idx < chunks.Length)
				{
					string cmd = chunks[idx++];

					switch (cmd)
					{
						case "Invert":
							Invert = true;
							break;

						default:
							TestName = cmd;
							break;
					}
				}

				return true;
			}

			public int GetOdds(Inventory inv)
			{
				Quality qual = Data.GetQuality(QualityName);
				int level = inv.GetCount(QualityName);
				if (qual != null)
					level = qual.GetLevel(level);

				if (TestName != null && Data.TestCurves.ContainsKey(TestName))
				{
					return Data.TestCurves[TestName](Value, ValueTwo, level);
				}

				int odds = Data.DefaultTestCurve(Value, ValueTwo, level);

				if (Invert)
					odds = 100-odds;

				return odds;
			}
		}

		public enum NodeType { Normal, PassThrough, Chance, Test }

		public string Name = "NoName";

		public string Title = "";
		public string Body = "";

		public string LinkTitle = "";
		public string LinkText = "";

		public NodeType Type = NodeType.Normal;

		public List<Requirement> Requirements = new List<Requirement>();
		public List<Inventory.Operation> Operations = new List<Inventory.Operation>();
		public string MoveLoc = null;
		public List<Link> Links = new List<Link>();
		public List<Test> Tests = new List<Test>();

		public bool IsLocation = false;
		public bool NoReturn = false;

		public bool ShouldShow(Inventory inv)
		{
			for (int i = 0; i < Requirements.Count; i++)
			{
				if (!Requirements[i].IsMet(inv) && Requirements[i].NoShow)
					return false;
			}

			return true;
		}

		public bool HasMet(Inventory inv)
		{
			for (int i = 0; i < Requirements.Count; i++)
			{
				Data.Log("HasMet: {0} {1} {2}", Name, Requirements[i].QualityName, Requirements[i].IsMet(inv));
				if (!Requirements[i].IsMet(inv))
					return false;
			}

			return true;
		}

		public int GetOdds(Inventory inv)
		{
			if (Tests.Count == 0)
				return 0;

			float odds = 1f;

			for (int i = 0; i < Tests.Count; i++)
			{
				odds *= (float)Tests[i].GetOdds(inv) / 100;
			}

			return (int)Math.Round(odds * 100);
		}
	}
}