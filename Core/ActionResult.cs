using System.Collections.Generic;
using System.Linq;
namespace Azalea.Core
{
	public class TestResult
	{
		public bool Success;
		public Storylet.Test Test;
	}

	public class ActionResult
	{
		public List<TestResult> Tests = new List<TestResult>();
		public bool TestSuccess { get { return Tests.All(x => x.Success); } }
		public Storylet Storylet;
		public List<Inventory.Result> InventoryOps = new List<Inventory.Result>();
		public Dictionary<string, Storylet.Link> Options = new Dictionary<string, Storylet.Link>();
		public List<Storylet.Link> NonOptions = new List<Storylet.Link>();
		public bool CountsAsLocation = false;
	}
}

