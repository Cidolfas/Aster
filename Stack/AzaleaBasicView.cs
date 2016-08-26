using System.IO;
using Azalea.Core;
using CommonMark;

namespace Azalea.Stack
{
	public static class AzaleaBasicView
	{
		public static string ReadFile(string path)
		{
			var f = File.OpenText(path);
			return f.ReadToEnd();
		}

		public static string Render(Game game, ActionResult result)
		{
			string body = "";

			if (game == null || game.CurrentStorylet == null)
				return "No storylet!";

			body += "<div class='storylet'>";

			body += string.Format("<h3>{0}</h3>\n", game.CurrentStorylet.Title);

			if (result != null && result.TestStorylet != null)
			{
				var qual = Data.GetQuality(result.TestStorylet.Tests[0].QualityName);
				if (qual != null)
					body += string.Format("<p>You {0} a test of {1} {2}</p>\n", (result.TestSuccess) ? "succeeded" : "failed", qual.Title, result.TestStorylet.Tests[0].Value);
			}

			body += CommonMarkConverter.Convert(game.CurrentStorylet.Body);

			body += "</div>";

			if (result != null)
			{
				foreach(var inv in result.InventoryOps)
				{
					body += RenderInventoryOp(inv);
				}
			}

			foreach (var kvp in game.Options)
			{
				body += RenderLink(kvp.Value, kvp.Key);
			}

			return body;
		}

		public static string RenderInventoryOp(Inventory.Result inv)
		{
			string result = "<div class='invOp'>";
			result += string.Format("<p>{0}</p>\n", GetInvOpString(inv));
			result += "</div>";

			return result;
		}

		public static string RenderLink(Storylet.Link l, string id)
		{
			var s = Data.GetStorylet(l.StoryletName);
			if (s == null)
				return "";

			string title = s.LinkTitle;
			string description = s.LinkText;
			if (l.Special == "Onwards")
			{
				title = "Onwards";
				description = null;
			}

			string result = "<div class='link'>";
			result += string.Format("<a href=\"#\" onclick=\"doAction('{0}'); return false;\">{1}</a>\n", id, title);
			if (!string.IsNullOrEmpty(description))
				result += string.Format("<p>{0}</p>", description);
			result += "</div>";

			return result;
		}

		public static string GetInvOpString(Inventory.Result res)
		{
			if (res == null)
				return "NoOpResult";

			string title = (res.Qual != null) ? res.Qual.Title : res.QualName;

			if (res.HasSet)
			{
				if (res.ModifiedDelta == 0)
				{
					return string.Format("{0} reamins at {1}", title, res.ModifiedEnd);
				}

				return string.Format("{0} is now {1}", title, res.ModifiedEnd);
			}
			if (res.HasGained)
			{
				return string.Format("You now have {0} {1}", res.ModifiedEnd, title);
			}
			if (res.HasLost)
			{
				return string.Format("You have lost all {0}", title);
			}
			if (res.HasIncreased)
			{
				if (res.ModifiedDelta > 0)
				{
					return string.Format("{0} has increased from {1} to {2}", title, res.ModifiedStart, res.ModifiedEnd);
				}

				return string.Format("{0} is increasing", title);
			}
			if (res.HasDecreased)
			{
				if (res.ModifiedDelta < 0)
				{
					return string.Format("{0} has decreased from {1} to {2}", title, res.ModifiedStart, res.ModifiedEnd);
				}

				return string.Format("{0} is decreasing", title);
			}

			return string.Format("{0} is unchanged", title);
		}
	}
}

