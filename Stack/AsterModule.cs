namespace Aster.Stack
{
	using Nancy;
	using Nancy.Responses;
	using Core;

	public class AsterModule : NancyModule
	{
		public AsterModule()
		{
			Get["/"] = _ => AsterBasicView.ReadFile("Stack/main.html");
			Get["/current"] = _ => AsterBasicView.Render(Data.CurrentGame, null);
			Get["/action/{id}"] = parameters =>
			{
				var result = Data.CurrentGame.TakeAction(parameters.id);
				if (result != null)
				{
					return AsterBasicView.Render(Data.CurrentGame, result);
				}

				string noAct = "No such action!<br/>\n";
				foreach(var kvp in Data.CurrentGame.Options)
				{
					noAct += string.Format("Action {0}: {1}<br/>\n", kvp.Key, kvp.Value.StoryletName);
				}

				return noAct;
			};
		}
	}
}
