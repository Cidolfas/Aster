using System;
using System.Collections.Generic;

namespace Azalea.Core
{
	public class Game
	{
		public Inventory Items = new Inventory();
		public ActionResult CurrentAR;
		public Storylet CurrentLocation;

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

		public bool TakeAction(string action)
		{
			if (CurrentAR == null)
			{
				Data.Log("Cannot take action {0}, no AR", action);
				return false;
			}

			Data.Log("Taking action {0}", action);
			if (CurrentAR.Options.ContainsKey(action))
			{
				var storylet = Data.GetStorylet(CurrentAR.Options[action].StoryletName);
				Data.Log("Storylet: {0} {1}", CurrentAR.Options[action].StoryletName, (storylet != null) ? "found" : "not found");
				if (storylet == null)
					return false;

				ActionResult r = new ActionResult();
				GoToStorylet(storylet, r);
				CurrentAR = r;
				SetupAR();
				return true;
			}

			return false;
		}

		public bool JumpToStorylet(Storylet s)
		{
			if (s == null)
			{
				Data.Log("Cannot jump to null storylet");
				return false;
			}

			Data.Log("Jumping to storylet {0}", s.Name);
			ActionResult r = new ActionResult();
			GoToStorylet(s, r);
			CurrentAR = r;
			SetupAR();

			return true;
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
			{
				CurrentLocation = s;
				r.CountsAsLocation = true;
			}

			if (s.MoveLoc != null)
			{
				var storylet = Data.GetStorylet(s.MoveLoc);
				if (storylet != null && storylet.IsLocation)
				{
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
				Data.Log("Chance!");

				int total = 0;
				var metStorylets = new List<Storylet>();
				var metLinks = new List<Storylet.Link>();
				foreach(var link in s.Links)
				{
					var storylet = Data.GetStorylet(link.StoryletName);
					if (storylet != null && storylet.HasMet(Items))
					{
						Data.Log("Add chance for {0} {1}", storylet.Name, link.Weight);

						total += link.Weight;
						metStorylets.Add(storylet);
						metLinks.Add(link);
					}
				}

				int roll = Data.GenericRandom.Next(total);
				Data.Log("Rolled {0} on total {1}", roll, total);
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
				int roll;
				foreach(var test in s.Tests)
				{
					bool success = true;
					int odds = 100 - test.GetOdds(Items);
					roll = Data.GenericRandom.Next(100);
					if (roll < odds)
						success = false;

					TestResult tr = new TestResult();
					tr.Test = test;
					tr.Success = success;
					r.Tests.Add(tr);
				}

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

			r.Storylet = s;
		}

		protected void SetupAR()
		{
			if (CurrentAR == null || CurrentAR.Storylet == null)
				return;

			foreach (var link in CurrentAR.Storylet.Links)
			{
				Storylet s = Data.GetStorylet(link.StoryletName);
				string linkText = (s != null) ? s.LinkText : "";
				Data.Log("Link {0} {1} {2}", link.StoryletName, linkText, s != null);
				if (s != null)
				{
					if (s.HasMet(Items))
					{
						string rnd = Data.GetRandomString();
						while (CurrentAR.Options.ContainsKey(rnd))
							rnd = Data.GetRandomString(); // Just in case
						CurrentAR.Options.Add(rnd, link);
					}
					else if (s.ShouldShow(Items))
					{
						CurrentAR.NonOptions.Add(link);
					}
				}
			}

			if (CurrentAR.Options.Count == 0 && CurrentAR.NonOptions.Count == 0)
			{
				Data.Log("Onwards {0}", CurrentLocation.Name);
				string rnd = Data.GetRandomString();
				CurrentAR.Options.Add(rnd, Storylet.Link.GetOnwards(CurrentLocation.Name));
			}
			else if (!CurrentAR.CountsAsLocation && !CurrentAR.Storylet.NoReturn && CurrentLocation != null)
			{
				Data.Log("Return {0}", CurrentLocation.Name);
				string rnd = Data.GetRandomString();
				while (CurrentAR.Options.ContainsKey(rnd))
					rnd = Data.GetRandomString();
				CurrentAR.Options.Add(rnd, Storylet.Link.GetReturn(CurrentLocation.Name));
			}
		}
	}
}