using Aster.Core;
using CommonMark;

namespace Aster.Stack
{
	public static class AsterBasicView
	{
		public static string Render(Game game, ActionResult result)
		{
			string body = "";

			if (game == null || game.CurrentStorylet == null)
				return "No storylet!";

			body += string.Format("<h3>{0}</h3>\n", game.CurrentStorylet.Title);

			if (result != null && result.TestStorylet != null)
			{
				var qual = Data.GetQuality(result.TestStorylet.TestQualityName);
				if (qual != null)
					body += string.Format("<p>You {0} a test of {1} {2}</p>\n", (result.TestSuccess) ? "succeeded" : "failed", qual.Title, result.TestStorylet.TestQualityAmount);
			}

			body += CommonMarkConverter.Convert(game.CurrentStorylet.Body);

			if (result != null)
			{
				foreach(var inv in result.InventoryOps)
				{
					body += string.Format("<p>{0}</p>\n", GetInvOpString(inv));
				}
			}

			foreach (var kvp in game.Options)
			{
				body += string.Format("<a href=\"/action/{0}\">{1}</a><br />\n", kvp.Key, kvp.Value.Text);
			}

			return body;
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

