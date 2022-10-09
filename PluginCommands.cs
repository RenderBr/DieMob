using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace DieMob
{
	class PluginCommands
	{
		public static void DieMobCommand(CommandArgs args)
		{
			if (args.Parameters.Count > 0 && args.Parameters[0].ToLower() == "reload")
			{
				if (Plugin.ReadConfig())
					args.Player.SendMessage("DieMob config reloaded.", Color.BurlyWood);
				else
					args.Player.SendErrorMessage("Error reading config. Check log for details.");
				return;
			}
			else if (args.Parameters.Count > 0 && args.Parameters[0].ToLower() == "list")
			{
				for (int r = 0; r < Database.DieMobRegions.Count; r++)
				{
					var regManReg = TShock.Regions.GetRegionByName(Database.DieMobRegions[r].TSRegion.Name);
					if (Database.DieMobRegions[r].TSRegion == null || regManReg == null || regManReg.Name == "")
					{
						Database.db.Query("DELETE FROM Database.DieMobRegions WHERE Region=@0 AND WorldID=@1", Database.DieMobRegions[r].TSRegion.Name, Main.worldID);
						Database.DieMobRegions.RemoveAt(r);
					}
				}

				int pageNumber;

				if (args.Parameters.Count < 2)
					pageNumber = 1;
				else if (!int.TryParse(args.Parameters[1], out pageNumber))
					args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}dm list <page number>", (args.Silent ? TShock.Config.Settings.CommandSilentSpecifier : TShock.Config.Settings.CommandSpecifier));

				if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber))
				{
					return;
				}
				IEnumerable<string> Regions = from region in Database.DieMobRegions
											  where region.TSRegion != null
											  select string.Format("{0} @ X: {1}, Y: {2}", region.TSRegion.Name, region.TSRegion.Area.X,
											  region.TSRegion.Area.Y);

				PaginationTools.SendPage(args.Player, pageNumber, PaginationTools.BuildLinesFromTerms(Regions),
					new PaginationTools.Settings
					{
						HeaderFormat = "DieMob Regions ({0}/{1}):",
						FooterFormat = "Type /dm list {0} for more."
					});
				return;
			}
			else if (args.Parameters.Count > 1 && args.Parameters[0].ToLower() == "info")
			{
				DieMobRegion reg = Database.GetRegionByName(args.Parameters[1]);
				if (reg == null)
					args.Player.SendMessage(String.Format("Region {0} not found on DieMob list", args.Parameters[1]), Color.Red);
				else
				{
					args.Player.SendMessage(String.Format("DieMob region: {0}", reg.TSRegion.Name), Color.DarkOrange);
					args.Player.SendMessage(String.Format("Type: {0}", reg.Type.ToString()), Color.LightSalmon);
					args.Player.SendMessage(String.Format("Affects friendly NPCs: {0}", reg.AffectFriendlyNPCs ? "True" : "False"), Color.LightSalmon);
					args.Player.SendMessage(String.Format("Affects statue spawned mobs: {0}", reg.AffectStatueSpawns ? "True" : "False"), Color.LightSalmon);
					args.Player.SendMessage(String.Format("Replacing {0} mobs. Type '{1}dm replacemobsinfo RegionName [pageNum]' to get a list.", reg.ReplaceMobs.Count, (args.Silent ? TShock.Config.Settings.CommandSilentSpecifier : TShock.Config.Settings.CommandSpecifier)), Color.LightSalmon);
				}
				return;
			}
			else if (args.Parameters.Count > 1 && (args.Parameters[0].ToLower() == "replacemobsinfo" || args.Parameters[0].ToLower() == "rminfo"))
			{
				DieMobRegion reg = Database.GetRegionByName(args.Parameters[1]);
				if (reg == null)
					args.Player.SendErrorMessage("Region {0} not found on DieMob list", args.Parameters[1]);
				else
				{
					int page = 0;
					if (args.Parameters.Count > 2)
						int.TryParse(args.Parameters[2], out page);
					if (page <= 0)
						page = 1;
					int startIndex = (page - 1) * 6;
					args.Player.SendMessage(String.Format("{0} mob replacements page {1}:", reg.TSRegion.Name, page), Color.LightSalmon);
					for (int i = startIndex; i < reg.ReplaceMobs.Count; i++)
					{
						if (i < startIndex + 6)
						{
							int key = reg.ReplaceMobs.Keys.ElementAt(i);
							args.Player.SendMessage(String.Format("[{0}] From: {1}  To: {2}", i + 1, key, reg.ReplaceMobs[key]), Color.BurlyWood);
						}
					}
				}
				return;
			}
			else if (args.Parameters.Count > 0 && args.Parameters[0].ToLower() == "mod")
			{
				if (args.Parameters.Count > 1)
				{
					DieMobRegion region = Database.GetRegionByName(args.Parameters[1]);
					if (region == null)
					{
						args.Player.SendErrorMessage("Region {0} not found on DieMob list", args.Parameters[1]);
						return;
					}
					if (args.Parameters.Count > 2)
					{
						switch (args.Parameters[2].ToLower())
						{
							case "type":
								{
									if (args.Parameters.Count > 3 && (args.Parameters[3].ToLower() == "kill" || args.Parameters[3].ToLower() == "repel" ||
										args.Parameters[3].ToLower() == "passive"))
									{
										if (args.Parameters[3].ToLower() == "repel")
										{
											region.Type = RegionType.Repel;
											args.Player.SendMessage(String.Format("Region {0} is now repeling mobs", region.TSRegion.Name), Color.LightSalmon);
										}
										else if (args.Parameters[3].ToLower() == "passive")
										{
											region.Type = RegionType.Passive;
											args.Player.SendMessage(String.Format("Region {0} is now passive", region.TSRegion.Name), Color.LightSalmon);
										}
										else
										{
											region.Type = RegionType.Kill;
											args.Player.SendMessage(String.Format("Region {0} is now killing mobs", region.TSRegion.Name), Color.LightSalmon);
										}
										Database.Diemob_Update(region);
										return;
									}
									break;
								}
							case "affectfriendlynpcs":
								{
									if (args.Parameters.Count > 3 && (args.Parameters[3].ToLower() == "true" || args.Parameters[3].ToLower() == "false"))
									{
										if (args.Parameters[3].ToLower() == "true")
										{
											region.AffectFriendlyNPCs = true;
											args.Player.SendMessage(String.Format("Region {0} is now affecting friendly NPCs", region.TSRegion.Name),
												Color.LightSalmon);
										}
										else
										{
											region.AffectFriendlyNPCs = false;
											args.Player.SendMessage(String.Format("Region {0} is no longer affecting friendly NPCs", region.TSRegion.Name),
												Color.LightSalmon);
										}
										Database.Diemob_Update(region);
										return;
									}
									break;
								}
							case "affectstatuespawns":
								{
									if (args.Parameters.Count > 3 && (args.Parameters[3].ToLower() == "true" || args.Parameters[3].ToLower() == "false"))
									{
										if (args.Parameters[3].ToLower() == "true")
										{
											region.AffectStatueSpawns = true;
											args.Player.SendMessage(String.Format("Region {0} is now affecting statue spawned mobs", region.TSRegion.Name),
												Color.LightSalmon);
										}
										else
										{
											region.AffectStatueSpawns = false;
											args.Player.SendMessage(String.Format("Region {0} is no longer affecting statue spawned mobs", region.TSRegion.Name),
												Color.LightSalmon);
										}
										Database.Diemob_Update(region);
										return;
									}
									break;
								}
							case "replacemobs":
								{
									if (args.Parameters.Count > 4 && (args.Parameters[3].ToLower() == "add" || args.Parameters[3].ToLower() == "del"))
									{
										int fromMobID;
										int toMobID;
										if (args.Parameters[3].ToLower() == "add" && args.Parameters.Count > 5 && int.TryParse(args.Parameters[4], out fromMobID) &&
											int.TryParse(args.Parameters[5], out toMobID))
										{
											if (region.ReplaceMobs.ContainsKey(fromMobID))
											{
												args.Player.SendMessage(String.Format("Region {0} already is already converting mobID {1} to mob {2}",
													region.TSRegion.Name, fromMobID, region.ReplaceMobs[fromMobID]), Color.LightSalmon);
												return;
											}
											region.ReplaceMobs.Add(fromMobID, toMobID);
											args.Player.SendMessage(String.Format("Region {0} is now converting mobs with id {1} to mobs {2}", region.TSRegion.Name,
												fromMobID, toMobID), Color.LightSalmon);
											Database.Diemob_Update(region);
											return;
										}
										else if (args.Parameters[3].ToLower() == "del" && int.TryParse(args.Parameters[4], out fromMobID))
										{
											if (region.ReplaceMobs.ContainsKey(fromMobID))
												region.ReplaceMobs.Remove(fromMobID);
											args.Player.SendMessage(String.Format("Region {0} is no longer converting mobs with id {1}", region.TSRegion.Name, fromMobID),
												Color.LightSalmon);
											Database.Diemob_Update(region);
											return;
										}
									}
									break;
								}
						}
					}
				}
				args.Player.SendMessage("{0}dm mod RegionName option arguments".SFormat(args.Silent ? TShock.Config.Settings.CommandSilentSpecifier : TShock.Config.Settings.CommandSpecifier), Color.DarkOrange);
				args.Player.SendMessage("Options:", Color.LightSalmon);
				args.Player.SendMessage("type - args: kill [default] / repel / passive", Color.LightSalmon);
				args.Player.SendMessage("affectfriendlynpcs - args: true / false [default]", Color.LightSalmon);
				args.Player.SendMessage("affectstatuespawns - args: true / false [default]", Color.LightSalmon);
				args.Player.SendMessage("replacemobs - args: add fromMobID toMobID / del fromMobID", Color.LightSalmon);
				return;
			}
			else if (args.Parameters.Count > 1)
			{
				var region = TShock.Regions.GetRegionByName(args.Parameters[1]);
				if (region != null && region.Name != "")
				{
					if (args.Parameters[0].ToLower() == "add")
					{
						if (Database.DieMobRegions.Select(r => r.TSRegion).Contains(region))
						{
							args.Player.SendMessage(String.Format("Region '{0}' is already on the DieMob list", region.Name), Color.LightSalmon);
							return;
						}
						if (!Database.DieMob_Add(region.Name))
						{
							args.Player.SendErrorMessage("Error adding '{0}' to DieMob list. Check log for details", region.Name);
							return;
						}
						Database.DieMobRegions.Add(new DieMobRegion(region));
						args.Player.SendMessage(String.Format("Region '{0}' added to DieMob list", region.Name), Color.BurlyWood);
						return;
					}
					else if (args.Parameters[0].ToLower() == "del")
					{
						if (!Database.DieMobRegions.Exists(r => r.TSRegion.Name == region.Name))
						{
							args.Player.SendMessage(String.Format("Region '{0}' is not on the DieMob list", region.Name), Color.LightSalmon);
							return;
						}
						Database.DieMob_Delete(region.Name);
						args.Player.SendMessage(String.Format("Region '{0}' deleted from DieMob list", region.Name), Color.BurlyWood);
						return;
					}
					return;
				}
				else
				{
					args.Player.SendErrorMessage($"Region '{args.Parameters[1]}' not found.");
					return;
				}
			}
			args.Player.SendMessage("Syntax: {0}diemob [add | del] RegionName - Creates / Deletes DieMob region based on pre-existing region".SFormat(args.Silent ? TShock.Config.Settings.CommandSilentSpecifier : TShock.Config.Settings.CommandSpecifier), Color.LightSalmon);
			args.Player.SendMessage("Syntax: {0}diemob list [page number] - Lists DieMob regions".SFormat(args.Silent ? TShock.Config.Settings.CommandSilentSpecifier : TShock.Config.Settings.CommandSpecifier), Color.LightSalmon);
			args.Player.SendMessage("Syntax: {0}diemob reload - Reloads config.json file".SFormat(args.Silent ? TShock.Config.Settings.CommandSilentSpecifier : TShock.Config.Settings.CommandSpecifier), Color.LightSalmon);
			args.Player.SendMessage("Syntax: {0}diemob mod RegionName - Modifies a DieMob region".SFormat(args.Silent ? TShock.Config.Settings.CommandSilentSpecifier : TShock.Config.Settings.CommandSpecifier), Color.LightSalmon);
			args.Player.SendMessage("Syntax: {0}diemob info RegionName - Displays info for a DieMob region".SFormat(args.Silent ? TShock.Config.Settings.CommandSilentSpecifier : TShock.Config.Settings.CommandSpecifier), Color.LightSalmon);
		}
	}
}
