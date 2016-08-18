using System.Collections.Generic;
namespace Azalea.Core
{
	public class ActionResult
	{
		public bool TestSuccess;
		public Storylet TestStorylet;
		public Storylet NewStorylet;
		public Storylet OnwardsStorylet;
		public Storylet CancelStorylet;
		public List<Inventory.Result> InventoryOps = new List<Inventory.Result>();
		public Storylet MoveLocation;
	}
}

