using System.Collections.Generic;

namespace Aster.Core
{
	public class Game
	{
		public Inventory Items = new Inventory();
		public Storylet CurrentStorylet;

		public Dictionary<string, Storylet.Link> Options = new Dictionary<string, Storylet.Link>();

		public ActionResult TakeAction(string action)
		{
			Data.Log("Taking action {0}", action);
			if (Options.ContainsKey(action))
			{
				var storylet = Data.GetStorylet(Options[action].StoryletName);
				Data.Log("Storylet: {0} {1}", Options[action].StoryletName, (storylet != null) ? "found" : "not found");
				if (storylet == null)
					return null;

				var result = new ActionResult();
				GoToStorylet(storylet, result);
				return result;
			}

			return null;
		}

		public void GoToStorylet(Storylet s, ActionResult r)
		{
			if (s == null)
				return;

			Data.Log("GoTo Storylet {0}", s.Name);

			foreach (var op in s.EntryOperations)
			{
				var opRes = op.ApplyOp(Items);
				r.InventoryOps.Add(opRes);
			}

			if (s.Type == Storylet.NodeType.PassThrough)
			{
				foreach(var link in s.Links)
				{
					var storylet = Data.GetStorylet(link.StoryletName);
					if (storylet != null && storylet.HasMet(Items))
					{
						GoToStorylet(storylet, r);
						return;
					}
				}

				return;
			}

			if (s.Type == Storylet.NodeType.Chance)
			{
				int total = 0;
				var metStorylets = new List<Storylet>();
				var metLinks = new List<Storylet.Link>();
				foreach(var link in s.Links)
				{
					var storylet = Data.GetStorylet(link.StoryletName);
					if (storylet != null && storylet.HasMet(Items))
					{
						total += link.Weight;
						metStorylets.Add(storylet);
						metLinks.Add(link);
					}
				}

				int roll = Data.GenericRandom.Next(total);
				for (int i = 0; i < metStorylets.Count; i++)
				{
					roll -= metLinks[i].Weight;
					if (roll < 0)
					{
						GoToStorylet(metStorylets[i], r);
						return;
					}
				}

				return;
			}

			if (s.Type == Storylet.NodeType.Test)
			{
				r.TestStorylet = s;

				Quality qual = Data.GetQuality(s.TestQualityName);
				int quantity = Items.GetCount(s.TestQualityName);
				if (qual != null)
					quantity = qual.GetLevel(quantity);

				int odds = 100 - s.GetOdds(quantity);
				int roll = Data.GenericRandom.Next(100);
				r.TestSuccess = (roll >= odds);

				int total = 0;
				var metStorylets = new List<Storylet>();
				var metLinks = new List<Storylet.Link>();
				foreach(var link in s.Links)
				{
					if (r.TestSuccess && link.Test != Storylet.Link.TestResult.Success)
						continue;

					if (!r.TestSuccess && link.Test != Storylet.Link.TestResult.Failure)
						continue;

					var storylet = Data.GetStorylet(link.StoryletName);
					if (storylet != null && storylet.HasMet(Items))
					{
						total += link.Weight;
						metStorylets.Add(storylet);
						metLinks.Add(link);
					}
				}

				roll = Data.GenericRandom.Next(total);
				for (int i = 0; i < metStorylets.Count; i++)
				{
					roll -= metLinks[i].Weight;
					if (roll < 0)
					{
						GoToStorylet(metStorylets[i], r);
						return;
					}
				}

				return;
			}

			CurrentStorylet = s;

			r.NewStorylet = s;
			r.OnwardsStorylet = Data.GetStorylet(s.OnwardsName);

			SetupStorylet();
		}

		protected void SetupStorylet()
		{
			Options.Clear();

			if (CurrentStorylet == null)
				return;

			// Data.Log("Links {0}", CurrentStorylet.Links.Count);

			foreach (var link in CurrentStorylet.Links)
			{
				Storylet s = Data.GetStorylet(link.StoryletName);
				Data.Log("Link {0} {1} {2}", link.StoryletName, link.Text, s != null);
				if (s != null && s.HasMet(Items))
				{
					string rnd = GetRandomString();
					while (Options.ContainsKey(rnd))
						rnd = GetRandomString(); // Just in case
					Options.Add(rnd, link);
				}
			}

			if (CurrentStorylet.OnwardsName != null)
			{
				Data.Log("Onwards {0}", CurrentStorylet.OnwardsName);
				string rnd = GetRandomString();
				while (Options.ContainsKey(rnd))
					rnd = GetRandomString();
				Options.Add(rnd, new Storylet.Link(CurrentStorylet.OnwardsName, "Onwards"));
			}
		}

		protected static string GetRandomString()
		{
			char[] availableChars = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
			int charCount = availableChars.Length;
			int num = 12;
			var result = new char[num];
			while (num-- > 0)
			{
				result[num] = availableChars[Data.GenericRandom.Next(charCount)];
			}
			return new string(result);
		}
	}
}