using System.Collections.Generic;

namespace Azalea.Core
{
	public class Quality
	{
		public string Name = "NoName";

		public string Title = "No Name";

		public string Description = "No Description";

		public string StoryletName;

		public List<string> Tags = new List<string>();

		public string PirmaryTag { get { return (Tags.Count > 0) ? Tags[0] : null; } }

		public bool HasStorylet { get { return !string.IsNullOrEmpty(StoryletName); } }

		public bool IsTag(string tagName) { return Tags.Contains(tagName); }
		
		public string Curve = null;

		public int GetLevel(int cp)
		{
			if (Curve == null)
				return cp;

			if (Data.LevelCurves.ContainsKey(Curve))
			{
				return Data.LevelCurves[Curve](cp);
			}

			return cp;
		}
	}
}