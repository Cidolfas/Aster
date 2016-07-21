namespace Aster.Stack
{
	using Nancy;
	using Core;

	public class AsterModule : NancyModule
	{
		public AsterModule()
		{
			Get["/"] = _ => AsterBasicView.Render(Data.CurrentGame);
			Get["/action/{id}"] = parameters =>
			{
				if (Data.CurrentGame.TakeAction(parameters.id))
				{
					return AsterBasicView.Render(Data.CurrentGame);
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
