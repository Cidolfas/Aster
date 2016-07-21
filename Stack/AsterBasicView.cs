using Aster.Core;
using CommonMark;

namespace Aster.Stack
{
	public static class AsterBasicView
	{
		public static string Render(Game game)
		{
			string body = "";

			if (game == null || game.CurrentStorylet == null)
				return "No storylet!";

			body += string.Format("<h3>{0}</h3>\n", game.CurrentStorylet.Title);

			body += CommonMarkConverter.Convert(game.CurrentStorylet.Body);

			foreach (var kvp in game.Options)
			{
				body += string.Format("<a href=\"/action/{0}\">{1}</a><br />\n", kvp.Key, kvp.Value.Text);
			}

			return body;
		}
	}
}

