using Auxiliary;
using SQLite;
using System.Collections.Generic;
using TShockAPI.DB;

namespace DieMob
{
	public class DieMobRegion
	{
		[PrimaryKey]
		public string Region { get; set; }

		public RegionType Type { get; set; }

		public DieMobRegion() { }

		public DieMobRegion(Region _reg)
		{
			Region = _reg.Name;
			Type = RegionType.Kill;
		}
	}
}
