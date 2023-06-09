using Auxiliary;
using System.Collections.Generic;
using TShockAPI.DB;

namespace DieMob
{
	public class DieMobRegion : BsonModel
	{
		private string _region;
		public string Region
		{
			get
			  => _region;
			set
			{
				_ = this.SaveAsync(x => x.Region, value);
				_region = value;
			}
		}

		private RegionType _type;
		public RegionType Type
		{
			get
			  => _type;
			set
			{
				_ = this.SaveAsync(x => x.Type, value);
				_type = value;
			}
		}

		private Dictionary<int, int> _replaceMobs;
		public Dictionary<int, int> ReplaceMobs
		{
			get
			  => _replaceMobs;
			set
			{
				_ = this.SaveAsync(x => x.ReplaceMobs, value);
				_replaceMobs = value;
			}
		}

		private bool _affectFriendlyNPCS;
		public bool AffectFriendlyNPCs
		{
			get
			  => _affectFriendlyNPCS;
			set
			{
				_ = this.SaveAsync(x => x.AffectFriendlyNPCs, value);
				_affectFriendlyNPCS = value;
			}
		}

		private bool _affectStatueSpawns;
		public bool AffectStatueSpawns
		{
			get
			  => _affectStatueSpawns;
			set
			{
				_ = this.SaveAsync(x => x.AffectStatueSpawns, value);
				_affectStatueSpawns = value;
			}
		}

		public DieMobRegion() { }

		public DieMobRegion(Region _reg)
		{
			Region = _reg.Name;
			Type = RegionType.Kill;
			ReplaceMobs = new Dictionary<int, int>();
			AffectFriendlyNPCs = false;
			AffectStatueSpawns = false;
		}
	}
}
