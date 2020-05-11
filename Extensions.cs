using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI.DB;

namespace DieMob
{
	public static class Extensions
	{
		public static bool InArea(this Region region, int x, int y) //overloaded with x,y
		{
			/*
			DO NOT CHANGE TO Area.Contains(x, y)!
			Area.Contains does not account for the right and bottom 'border' of the rectangle,
			which results in regions being trimmed.
			*/
			return x >= region.Area.X && x <= region.Area.X + (region.Area.Width + Plugin.Config.RegionStretch) && y >= region.Area.Y && y <= region.Area.Y + (region.Area.Height + Plugin.Config.RegionStretch);
		}
	}
}
