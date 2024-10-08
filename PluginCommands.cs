﻿using CSF;
using CSF.TShock;
using System.Threading.Tasks;
using TShockAPI;

namespace DieMob
{
    [RequirePermission("diemob.use")]
    class PluginCommands : TSModuleBase<TSCommandContext>
    {
        [Command("diemob", "dm")]
        [Description("DieMob base command")]
        public async Task<IResult> DieMobCommand(string sub = "", string args1 = "", string args2 = "")
        {
            switch (sub.ToLower())
            {
                case "reload":
                    Plugin.api.ReloadDieMob();
                    return Success("DieMob config reloaded independantly.");
                case "wipe":
                case "clear":
                {
                    var regions = await Plugin.api.RetrieveAllRegions();
                    foreach (DieMobRegion r in regions)
                    {
                        await Plugin.api.DeleteDiemob(r.Region);
                    }

                    return Success("All DieMob regions cleared.");
                }
                case "list":
                {
                    var regions = await Plugin.api.RetrieveAllRegions();

                    foreach (DieMobRegion r in regions)
                    {
                        if (TShock.Regions.GetRegionByName(r.Region) == null)
                            await Plugin.api.DeleteDiemob(r.Region);
                    }

                    int pageNumber;

                    if (string.IsNullOrWhiteSpace(args1))
                        pageNumber = 1;
                    else
                    {
                        try
                        {
                            pageNumber = int.Parse(args1);
                        }
                        catch
                        {
                            return Error(
                                "Couldn't parse page number, please make sure you are entering a valid number!");
                        }
                    }

                    PaginationTools.SendPage(Context.Player, pageNumber, PaginationTools.BuildLinesFromTerms(regions),
                        new PaginationTools.Settings
                        {
                            HeaderFormat = "DieMob Regions ({0}/{1}):",
                            FooterFormat = "Type /diemob list {0} for more.",
                            NothingToDisplayString = "There are currently no diemob regions!"
                        });
                    return ExecuteResult.FromSuccess();
                }
                case "create":
                case "add":
                {
                    if (!string.IsNullOrEmpty(args1))
                    {
                        if (!TShock.Regions.GetRegionByName(args1).Equals(null))
                        {
                            if (await Plugin.api.RetrieveRegion(args1) == null)
                            {
                                await Plugin.api.CreateDieMobRegion(args1);
                                return Success($"DieMob region {args1} added.");
                            }
                            else return Error($"DieMob region {args1} already exists.");
                        }
                        else return Error("That is not a defined TShock region!");
                    }
                    else return Error("Please enter a valid region name!");
                }
                case "delete":
                case "remove":
                case "rem":
                case "del":
                {
                    if (!string.IsNullOrEmpty(args1))
                    {
                        if (!TShock.Regions.GetRegionByName(args1).Equals(null))
                        {
                            await Plugin.api.DeleteDiemob(args1);
                            return Success($"You successfully deleted {args1} from diemob!");
                        }
                        else return Error("Please enter a valid region name!");
                    }
                    else return Error("Please enter a valid region name!");
                }
                case "type":
                {
                    if (string.IsNullOrWhiteSpace(args1))
                    {
                        return Error("Please enter a valid region name!");
                    }

                    if (string.IsNullOrWhiteSpace(args2))
                    {
                        return Error("Please enter a valid region type: 'normal', or 'repel'.");
                    }

                    if (!TShock.Regions.GetRegionByName(args1).Equals(null))
                    {
                        var diemobRegion = await Plugin.api.RetrieveRegion(args1);

                        if (diemobRegion == null)
                        {
                            return Error("That region is not a DieMob region.");
                        }
                        
                        if (args2.ToLower() == "normal")
                        {
                            diemobRegion.Type = RegionType.Kill;
                            await Plugin.api.SQL.UpdateAsync(diemobRegion);
                            return Success($"Region {args1} is now a normal DieMob region.");
                        }
                        else if (args2.ToLower() == "repel")
                        {
                            diemobRegion.Type = RegionType.Repel;
                            await Plugin.api.SQL.UpdateAsync(diemobRegion);
                            return Success($"Region {args1} is now a repel DieMob region.");
                        }
                        else
                        {
                            return Error("Please enter a valid region type: 'normal', or 'repel'.");
                        }
                    }

                    return ExecuteResult.FromSuccess();
                }
                case "":
                case "help":
                {
                    Info("Diemob Commands");
                    Info("/diemob add <region> - adds a region to diemob");
                    Info("/diemob clear - clears all diemob regions");
                    Info("/diemob del <region> - removes a region from diemob");
                    Info("/diemob help - displays this page");
                    Info("/diemob list <page> - gets a list of diemob regions");
                    Info("/diemob type <region> <type> - sets the type of a region to 'normal' or 'repel'");
                    return Info("/diemob reload - reloads the DieMob configuration.");
                }
                default:
                    return Error("Invalid subcommand. Type /diemob help for a list of subcommands.");
            }
        }
    }
}