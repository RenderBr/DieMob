using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI.DB;

namespace DieMob
{
	public class DieMobRegion
	{
		public Region TSRegion;
		public RegionType Type;
		public Dictionary<int, int> ReplaceMobs;
		public bool AffectFriendlyNPCs;
		public bool AffectStatueSpawns;

		public DieMobRegion(Region _reg)
		{
			TSRegion = _reg;
			Type = RegionType.Kill;
			ReplaceMobs = new Dictionary<int, int>();
			AffectFriendlyNPCs = false;
			AffectStatueSpawns = false;
		}
	}
}
