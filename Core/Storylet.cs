using System;
using System.Collections.Generic;

namespace Aster.Core
{
	public class Storylet
	{
		public class Link
		{
			public enum TestResult { None, Success, Failure }

			public string Text;
			public string StoryletName;
			public int Weight = 1;
			public TestResult Test = TestResult.None;

			public Link(string sName, string txt)
			{
				Text = txt;
				StoryletName = sName;
			}

			public Link(string sName, int weight)
			{
				StoryletName = sName;
				Weight = weight;
			}

			public Link(string sName, string result, int weight)
			{
				StoryletName = sName;
				Weight = weight;

				if (result == "Success")
					Test = TestResult.Success;
				else if (result == "Failure")
					Test = TestResult.Failure;
			}
		}

		public class Requirement
		{
			public enum ReqType { None, Equals, LessThan, GreaterThan, Has, HasNo, Range }

			public ReqType Req;
			public string QualityName;
			public int Value;
			public int ValueTwo;
			public bool NoShow; // Controls if we should show this storylet or not based on this req
			public bool Hidden; // Controls if this req should be visible to the player

			public bool FromLine(string[] chunks)
			{
				if (chunks.Length < 3)
					return false;

				if (chunks[0] != "Req:")
					return false;

				QualityName = chunks[1];

				if (!Enum.TryParse<ReqType>(chunks[2], true, out Req))
					return false;

				switch (Req)
				{
					case ReqType.None:
						return false;

					case ReqType.Has:
					case ReqType.HasNo:
						NoShow = (chunks.Length == 4 && chunks[3] == "NoShow");
						return true;

					case ReqType.Equals:
					case ReqType.LessThan:
					case ReqType.GreaterThan:
						if (chunks.Length < 4)
							return false;

						if (!int.TryParse(chunks[3], out Value))
							return false;

						NoShow = (chunks.Length == 5 && chunks[4] == "NoShow");

						return true;

					case ReqType.Range:
						if (chunks.Length < 5)
							return false;

						if (!int.TryParse(chunks[3], out Value) || !int.TryParse(chunks[4], out ValueTwo))
							return false;

						NoShow = (chunks.Length == 6 && chunks[5] == "NoShow");

						return true;

					default:
						return false;
				}
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

					case ReqType.GreaterThan:
						return quantity > Value;

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

		public enum NodeType { Normal, PassThrough, Chance, Test }

		public string Name = "NoName";

		public string Title = "NoTitle";
		public string Body = "NoBody";

		public NodeType Type = NodeType.Normal;

		public List<Requirement> Requirements = new List<Requirement>();
		public List<Inventory.Operation> EntryOperations = new List<Inventory.Operation>();
		public List<Link> Links = new List<Link>();
		public string OnwardsName;
		public string TestQualityName;
		public int TestQualityAmount;
		public bool TestNarrow;

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

		public int GetOdds(int level)
		{
			if (TestNarrow)
			{
				int delta = level - TestQualityAmount;
				return Math.Max(0, Math.Min(100, 60 + 10 * delta));
			}

			return Math.Max(0, Math.Min(100, (int)Math.Round(0.6 * ((double)level / TestQualityAmount))));
		}
	}
}