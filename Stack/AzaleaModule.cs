namespace Azalea.Stack
{
	using Nancy;
	using Core;

	public class AzaleaModule : NancyModule
	{
		public AzaleaModule()
		{
			Get["/"] = _ => AzaleaBasicView.ReadFile("Stack/main.html");
			Get["/current"] = _ => AzaleaBasicView.Render(Data.CurrentGame, null);
			Get["/action/{id}"] = parameters =>
			{
				var result = Data.CurrentGame.TakeAction(parameters.id);
				if (result != null)
				{
					return AzaleaBasicView.Render(Data.CurrentGame, result);
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
