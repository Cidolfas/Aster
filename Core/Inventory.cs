using System.Collections.Generic;

namespace Aster.Core
{
	public class Inventory
	{
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