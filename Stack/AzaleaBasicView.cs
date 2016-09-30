using System.IO;
using System.Collections.Generic;
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

			if (game == null || game.CurrentAR == null || game.CurrentAR.Storylet == null)
				return "No storylet!";

			body += "<div class='storylet'>";

			body += string.Format("<h3>{0}</h3>\n", game.CurrentAR.Storylet.Title);

			foreach(var test in game.CurrentAR.Tests)
			{
				if (test != null && test.Test != null)
				{
					var qual = Data.GetQuality(test.Test.QualityName);
					if (qual != null)
						body += string.Format("<p>You {0} a test of {1} {2}</p>\n", (test.Success) ? "succeeded" : "failed", qual.Title, test.Test.Value);
				}
			}

			body += CommonMarkConverter.Convert(game.CurrentAR.Storylet.Body);

			body += "</div>";

			if (result != null)
			{
				foreach(var inv in result.InventoryOps)
				{
					body += RenderInventoryOp(inv);
				}
			}

			List<KeyValuePair<string, Storylet.Link>> specialLinks = new List<KeyValuePair<string, Storylet.Link>>();

			foreach (var kvp in game.CurrentAR.Options)
			{
				if (string.IsNullOrEmpty(kvp.Value.Special))
				{
					body += RenderLink(kvp.Value, kvp.Key, game.Items);
				}
				else
				{
					specialLinks.Add(kvp);
				}
			}

			foreach (var link in game.CurrentAR.NonOptions)
			{
				body += RenderLink(link, null, game.Items);
			}

			foreach (var kvp in specialLinks)
			{
				body += RenderLink(kvp.Value, kvp.Key, game.Items);
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

		public static string RenderLink(Storylet.Link l, string id, Inventory inv)
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

			if (l.Special == "Return")
			{
				title = "Return";
				description = null;
			}

			bool valid = !string.IsNullOrEmpty(id);

			string result = string.Format("<div class='{0}'>", valid ? "link" : "link inactive");
			if (valid)
			{
				result += string.Format("<a href=\"#\" onclick=\"doAction('{0}'); return false;\">{1}</a>\n", id, title);
			}
			else
			{
				result += string.Format("<span class='nonoption'>{0}</span>\n", title);
			}

			if (!string.IsNullOrEmpty(description))
				result += string.Format("<p>{0}</p>\n", description);

			if (string.IsNullOrEmpty(l.Special))
			{
				if (s.Type == Storylet.NodeType.Test && s.Tests.Count > 0)
				{
					result += "<p class='tests'>\n";
					foreach(var test in s.Tests)
					{
						result += string.Format("{0}<br />\n", GetTestString(test, inv));
					}
					result += "</p>\n";
				}

				if (s.Requirements.Count > 0)
				{
					result += "<p class='reqs'>\n";
					foreach(var req in s.Requirements)
					{
						result += string.Format("{0}<br />\n", GetReqString(req, inv));
					}
					result += "</p>\n";
				}
			}

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

		public static string GetTestString(Storylet.Test test, Inventory inv)
		{
			if (test == null || inv == null)
				return "";

			Quality q = Data.GetQuality(test.QualityName);
			string qName = (q != null) ? q.Name : test.QualityName;

			int odds = test.GetOdds(inv);

			return string.Format("Test on {0} {1}%", qName, odds);
		}

		public static string GetReqString(Storylet.Requirement req, Inventory inv)
		{
			if (req == null || inv == null)
				return "";

			if (req.NoShow) // This controls storylet visibility, not IsMet
				return "";

			bool met = req.IsMet(inv);

			string classname = met ? "met" : "notmet";

			string countword = "";
			switch(req.Req)
			{
				case Storylet.Requirement.ReqType.AtLeast:
					countword = "you have at least " + req.Value;
					break;

				case Storylet.Requirement.ReqType.Equals:
					countword = "you have exactly " + req.Value;
					break;

				case Storylet.Requirement.ReqType.Has:
					countword = "you have";
					break;

				case Storylet.Requirement.ReqType.HasNo:
					countword = "you have no";
					break;

				case Storylet.Requirement.ReqType.LessThan:
					countword = "you have less than " + req.Value;
					break;

				case Storylet.Requirement.ReqType.Range:
					countword = "you have between " + req.Value + " and " + req.ValueTwo;
					break;
			}

			Quality q = Data.GetQuality(req.QualityName);
			string qName = (q != null) ? q.Name : req.QualityName;

			int count = inv.GetCount(req.QualityName);
			string currentCount = (q != null) ? q.GetLevel(count).ToString() : count.ToString();

			return string.Format("<span class='{0}'>Requires {1} {2} (You have {3})</span>", classname, countword, qName, currentCount);
		}
	}
}

