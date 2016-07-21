using System;
using System.Collections.Generic;

namespace Aster.Core
{
	public class Storylet
	{
		public class Link
		{
			public string Text;
			public string StoryletName;
			public int Weight = 1;

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
		}

		public class Requirement
		{
			public enum ReqType { None, Equals, LessThan, GreaterThan, Has, HasNo, Range }

			public ReqType Req;
			public string QualityName;
			public int Value;
			public int ValueTwo;
			public bool NoShow;

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
				int quantity = inv.GetCount(QualityName);

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

		public class InventoryOp
		{
			public enum OpType { None, Gain, Lose, Set, GainRange, LoseRange }

			public OpType Op;
			public string QualityName;
			public int Value;
			public int ValueTwo;

			public bool FromLine(string line)
			{
				string[] chunks = line.Split(' ');
				if (chunks.Length < 4)
					return false;

				if (chunks[0] != "Q:")
					return false;

				QualityName = chunks[1];

				if (!Enum.TryParse<OpType>(chunks[2], true, out Op))
					return false;

				if (!int.TryParse(chunks[3], out Value))
					return false;

				if ((Op == OpType.GainRange || Op == OpType.LoseRange) && ((chunks.Length < 5) || !int.TryParse(chunks[4], out ValueTwo)))
					return false;

				return Op != OpType.None;
			}

			public void ApplyOp(Inventory Items)
			{
				int before = Items.GetCount(QualityName);
				switch(Op)
				{
					case Storylet.InventoryOp.OpType.Gain:
						Items.AddItem(QualityName, Value);
						break;

					case Storylet.InventoryOp.OpType.Lose:
						Items.RemoveItem(QualityName, Value);
						break;

					case Storylet.InventoryOp.OpType.Set:
						Items.SetItem(QualityName, Value);
						break;

					case Storylet.InventoryOp.OpType.GainRange:
						int v = Data.GenericRandom.Next(Value, ValueTwo);
						Items.AddItem(QualityName, v);
						break;

					case Storylet.InventoryOp.OpType.LoseRange:
						v = Data.GenericRandom.Next(Value, ValueTwo);
						Items.RemoveItem(QualityName, v);
						break;
				}
				int after = Items.GetCount(QualityName);
				Data.Log("ApplyOp: {0} {1} from {2} to {3}", QualityName, Op, before, after);
			}
		}

		public enum NodeType { Normal, PassThrough, Chance }

		public string Name = "NoName";

		public string Title = "NoTitle";
		public string Body = "NoBody";

		public NodeType Type = NodeType.Normal;

		public List<Requirement> Requirements = new List<Requirement>();
		public List<InventoryOp> EntryOperations = new List<InventoryOp>();
		public List<Link> Links = new List<Link>();
		public string OnwardsName;

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
	}
}