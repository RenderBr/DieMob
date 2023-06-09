using Auxiliary;
using Auxiliary.Configuration;
using CSF;
using CSF.TShock;
using DieMob.Api;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace DieMob
{
    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin
    {
        #region Plugin Metadata
        public override string Name
            => "DieMob Regions";

        public override string Author
            => "Average";

        public override string Description
            => "Removes any entities that decide to enter a region";

        public override Version Version
            => new Version(1, 1);

		#endregion

		#region Plugin Initialization
		public Plugin(Main game)
            : base(game)
        {
            Order = 1;
            // Define the command standardization framework made for TShock.
            _fx = new(new CommandConfiguration()
            {
                DoAsynchronousExecution = false
            });
        }
        private readonly TSCommandFramework _fx;
        private static DateTime lastUpdate = DateTime.UtcNow;
        public static DiemobSettings Settings;
        public static DiemobApi api;

        public async override void Initialize()
        {
            Configuration<DiemobSettings>.Load(nameof(DieMob));
            Settings = Configuration<DiemobSettings>.Settings;
            await _fx.BuildModulesAsync(typeof(Plugin).Assembly);

            GeneralHooks.ReloadEvent += (x) =>
            {
                Configuration<DiemobSettings>.Load(nameof(DieMob));
                x.Player.SendSuccessMessage("[Diemob] Reloaded configuration.");
            };

            TerrariaApi.Server.ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);
            RegionHooks.RegionDeleted += OnRegionDelete;
            api = new DiemobApi();


        }
        #endregion


        #region Hooks
        private async void OnRegionDelete(RegionHooks.RegionDeletedEventArgs args)
        {
            DieMobRegion region = await api.RetrieveRegion(args.Region.Name);

            if (region != null)
                api.DeleteDiemob(args.Region.Name);
        }

        private async void OnUpdate(EventArgs e)
        {
            if ((DateTime.UtcNow - lastUpdate).TotalMilliseconds >= Settings.UpdateInterval)
            {
                lastUpdate = DateTime.UtcNow;
                List<DieMobRegion> regions = StorageProvider.GetMongoCollection<DieMobRegion>("DieMobRegions").Find(x => true).ToList();
                foreach (var r in regions)
                {
                    var region = TShock.Regions.GetRegionByName(r.Region);
                    if (region is null)
                        continue;

                    foreach (var npc in Main.npc)
                    {
                        if (npc.active && !npc.townNPC)
                        {
                            if (!region.InArea((int)(npc.position.X / 16), (int)(npc.position.Y / 16)))
                                continue;

                            if (region.Area.Contains(npc.Center.ToTileCoordinates()))
                            {
                                npc.active = false;
                                npc.life = 0;
                                npc.lifeMax = 0;
                                npc.checkDead();
                                Main.npc[npc.whoAmI] = new NPC();
                                NetMessage.SendData((int)PacketTypes.NpcUpdate, -1, -1, NetworkText.Empty, npc.whoAmI);
                            }
                        }
                    }


                }

            }
        }
        #endregion

    }

}
