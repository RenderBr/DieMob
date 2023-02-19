using CSF;
using CSF.TShock;
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
	[RequirePermission("diemob.use")]
	class PluginCommands : TSModuleBase<TSCommandContext>
	{
		[Command("diemob")]
		[Description("DieMob command")]
		public async Task<IResult> DieMobCommand(string sub = "", string args1 = "", string args2 = "", string args3 = "")
		{
			switch (sub.ToLower())
			{
				case "list":
					{
						var regions = Plugin.api.RetrieveAllRegions();

						foreach (DieMobRegion r in regions)
						{
							if (TShock.Regions.GetRegionByName(r.Region) == null)
								Plugin.api.DeleteDiemob(r.Region);
						}

						int pageNumber;

						if (args1 == "")
							pageNumber = 1;
						else
						{
							try
							{
								pageNumber = int.Parse(args1);

							}
							catch
							{
								return Error("Couldn't parse page number, please make sure you are entering a valid number!");
							}
						}

						PaginationTools.SendPage(Context.Player, pageNumber, PaginationTools.BuildLinesFromTerms(regions),
							new PaginationTools.Settings
							{
								HeaderFormat = "DieMob Regions ({0}/{1}):",
								FooterFormat = "Type /dm list {0} for more."
							});
						return ExecuteResult.FromSuccess();
					}
				case "create":
				case "add":
					{
						if (args1 != "")
						{
							if (!TShock.Regions.GetRegionByName(args1).Equals(null))
							{
								await Plugin.api.CreateDieMobRegion(args1);
								return Success($"You successfully added {args1} as a diemob region!");
							}
							else
							{
								return Error("Please enter a valid region name!");
							}

						}
						else
						{
							return Error("Please enter a valid region name!");
						}
					}
				case "delete":
				case "remove":
				case "rem":
                case "del":
                    {
                        if (args1 != "")
                        {
                            if (!TShock.Regions.GetRegionByName(args1).Equals(null))
                            {
                                Plugin.api.DeleteDiemob(args1);
                                return Success($"You successfully deleted {args1} from diemob!");
                            }
                            else
                            {
                                return Error("Please enter a valid region name!");
                            }

                        }
                        else
                        {
                            return Error("Please enter a valid region name!");
                        }
                    }
				default:
					{
						Info("Diemob Commands");
						Info("/diemob add <region> - adds a region to diemob");
						Info("/diemob del <region> - removes a region from diemob");
						return Info("/diemob list <page> - gets a list of diemob regions");
					}

            }

		}
	}
}
