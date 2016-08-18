using System;
using System.Collections.Generic;

namespace Azalea.Core
{
	public class Game
	{
		public Inventory Items = new Inventory();
		public Storylet CurrentStorylet;
		public Storylet CurrentLocation;

		public Dictionary<string, Storylet.Link> Options = new Dictionary<string, Storylet.Link>();

		public virtual void Init()
		{
			Data.Init();

			// Setup odds curves
			Data.TestCurves.Add("Broad", BroadOdds);
			Data.TestCurves.Add("Narrow", NarrowOdds);
			Data.TestCurves.Add("Range", RangeOdds);
			Data.DefaultTestCurve = BroadOdds;

			// Setup level curves
			Data.LevelCurves.Add("Straight", StraightLevel);
			Data.LevelCurves.Add("Attribute", AttributeLevel);
			Data.DefaultLevelCurve = StraightLevel;
		}

#region Odds

		protected int BroadOdds(int v1, int v2, int q)
		{
			return Math.Max(0, Math.Min(100, (int)Math.Round(60 * ((double)q / v1))));
		}

		protected int NarrowOdds(int v1, int v2, int q)
		{
			int delta = q - v1;
			return Math.Max(0, Math.Min(100, 60 + 10 * delta));
		}

		protected int RangeOdds(int v1, int v2, int q)
		{
			if (v2 < 0)
				v2 = v1;

			if (q >= v2)
				return 100;

			if (q <= v1)
				return 0;

			return  Math.Max(0, Math.Min(100, (int)Math.Round(100 * ((double)q - v1) / (v2 - v1))));
		}

#endregion

#region Levels
	
		protected int StraightLevel(int q)
		{
			return q;
		}

		protected int AttributeLevel(int q)
		{
			int level = 0;
			int i = 0;
			while(i < q)
			{
				level++;
				i += level;
			}
			return (i == q) ? level : level-1;
		}

#endregion

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

			foreach (var op in s.Operations)
			{
				var opRes = op.ApplyOp(Items);
				r.InventoryOps.Add(opRes);
			}

			if (s.IsLocation)
				CurrentLocation = s;

			if (s.MoveLoc != null)
			{
				var storylet = Data.GetStorylet(s.MoveLoc);
				if (storylet != null && storylet.IsLocation)
				{
					r.MoveLocation = storylet;
					CurrentLocation = storylet;
				}
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

				int roll;
				bool success = true;
				foreach(var test in s.Tests)
				{
					int odds = 100 - test.GetOdds(Items);
					roll = Data.GenericRandom.Next(100);
					if (roll < odds)
						success = false;
				}
				r.TestSuccess = success;

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
				Data.Log("Link {0} {1} {2}", link.StoryletName, s.LinkText, s != null);
				if (s != null && s.HasMet(Items))
				{
					string rnd = Data.GetRandomString();
					while (Options.ContainsKey(rnd))
						rnd = Data.GetRandomString(); // Just in case
					Options.Add(rnd, link);
				}
			}

			if (Options.Count == 0)
			{
				Data.Log("Onwards {0}", CurrentLocation.Name);
				string rnd = Data.GetRandomString();
				Options.Add(rnd, Storylet.Link.GetOnwards(CurrentLocation.Name));
			}
		}
	}
}