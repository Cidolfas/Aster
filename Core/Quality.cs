using System.Collections.Generic;

namespace Aster.Core
{
	public class Quality
	{
		public enum QType { None, Item, Attribute, Flag }

		public QType Type = QType.Item;

		public string Name = "NoName";

		public string Title = "No Name";

		public string Description = "No Description";

		public string StoryletName;

		public List<string> Tags = new List<string>();

		public bool HasStorylet { get { return !string.IsNullOrEmpty(StoryletName); } }

		public bool IsTag(string tagName) { return Tags.Contains(tagName); }

		public int GetLevel(int cp)
		{
			if (Type == QType.Attribute)
			{
				int level = 0;
				int i = 0;
				while(i < cp)
				{
					level++;
					i += level;
				}
				return (i == cp) ? level : level-1;
			}

			return cp;
		}
	}
}