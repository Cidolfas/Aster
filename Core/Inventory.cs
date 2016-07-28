using System;
using System.Collections.Generic;

namespace Aster.Core
{
	public class Inventory
	{
		public class Operation
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

			public Result ApplyOp(Inventory Items)
			{
				Result res = new Result();
				int before = Items.GetCount(QualityName);
				switch(Op)
				{
					case OpType.Gain:
						Items.AddItem(QualityName, Value);
						break;

					case OpType.Lose:
						Items.RemoveItem(QualityName, Value);
						break;

					case OpType.Set:
						Items.SetItem(QualityName, Value);
						break;

					case OpType.GainRange:
						int v = Data.GenericRandom.Next(Value, ValueTwo);
						Items.AddItem(QualityName, v);
						break;

					case OpType.LoseRange:
						v = Data.GenericRandom.Next(Value, ValueTwo);
						Items.RemoveItem(QualityName, v);
						break;
				}
				int after = Items.GetCount(QualityName);
				Data.Log("ApplyOp: {0} {1} from {2} to {3}", QualityName, Op, before, after);

				res.Qual = Data.GetQuality(QualityName);
				res.QualName = QualityName;
				res.Start = before;
				res.End = after;
				res.Op = Op;
				return res;
			}
		}

		public class Result
		{
			public Quality Qual;
			public string QualName;
			public int Start;
			public int End;
			public Operation.OpType Op;

			public bool HasIncreased { get { return (Op != Operation.OpType.Set) && (End > Start); } }
			public bool HasDecreased { get { return (Op != Operation.OpType.Set) && (End < Start); } }
			public bool HasSet { get { return (Op == Operation.OpType.Set); } }
			public bool HasGained { get { return (Start == 0 && End > 0); } }
			public bool HasLost { get { return (Start > 0 && End == 0); } }
			public int Delta { get { return End - Start; } }
			public int ModifiedStart { get { return (Qual != null) ? Qual.GetLevel(Start) : Start; } }
			public int ModifiedEnd { get { return (Qual != null) ? Qual.GetLevel(End) : End; } }
			public int ModifiedDelta { get { return ModifiedEnd - ModifiedStart; } }
		}

		protected Dictionary<string, int> mItems = new Dictionary<string, int>();

		public int GetCount(string name)
		{
			if (mItems.ContainsKey(name))
				return mItems[name];
				
			return 0;
		}

		public void AddItem(string name, int count)
		{
			if (mItems.ContainsKey(name))
				mItems[name] += count;
			else
				mItems.Add(name, count);
		}

		public void RemoveItem(string name, int count = 1, bool all = false)
		{
			if (!mItems.ContainsKey(name))
				return;

			if (all || (mItems[name] <= count))
			{
				mItems.Remove(name);
			}
			else
			{
				mItems[name] -= count;
			}
		}

		public void SetItem(string name, int count)
		{
			if (mItems.ContainsKey(name))
				mItems[name] = count;
			else
				mItems.Add(name, count);
		}
	}
}