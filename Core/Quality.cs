using System.Collections.Generic;

namespace Aster.Core
{
	public class Quality
	{
		public string Name = "NoName";

		public string Title = "No Name";

		public string Description = "No Description";

		public string StoryletName;

		public List<string> Tags = new List<string>();

		public bool HasStorylet { get { return !string.IsNullOrEmpty(StoryletName); } }

		public bool IsTag(string tagName) { return Tags.Contains(tagName); }
	}
}