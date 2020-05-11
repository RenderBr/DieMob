using Microsoft.Xna.Framework;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Hooks;

namespace DieMob
{


	[ApiVersion(2, 1)]
	public class Plugin : TerrariaPlugin
	{
		#region Info
		public override string Name
		{
			get { return "DieMob Regions"; }
		}
		public override string Author
		{
			get { return "Zaicon"; }
		}
		public override string Description
		{
			get { return "Adds monster protection option to regions"; }
		}
		public override Version Version
		{
			get { return Assembly.GetExecutingAssembly().GetName().Version; }
		}
		#endregion

		#region Initialize
		public Plugin(Main game)
			: base(game)
		{
			Order = 1;
		}

		private static string savepath = Path.Combine(TShock.SavePath, "DieMob/");
		private static bool initialized = false;
		private static DateTime lastUpdate = DateTime.UtcNow;
		private static Config config;

		public override void Initialize()
		{
			ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);
			ServerApi.Hooks.GameInitialize.Register(this, OnInitialize, 1);
			RegionHooks.RegionDeleted += OnRegionDelete;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
				ServerApi.Hooks.GameUpdate.Deregister(this, OnUpdate);
				RegionHooks.RegionDeleted -= OnRegionDelete;
			}
			base.Dispose(disposing);
		}

		#endregion
		public static void CreateConfig()
		{
			string filepath = Path.Combine(savepath, "config.json");

			try
			{
				File.WriteAllText(filepath, JsonConvert.SerializeObject(new Config(), Formatting.Indented));
			}
			catch (Exception ex)
			{
				TShock.Log.ConsoleError(ex.Message);
				config = new Config();
			}
		}
		public static bool ReadConfig()
		{
			string filepath = Path.Combine(savepath, "config.json");
			try
			{
				if (File.Exists(filepath))
				{
					config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(filepath));
					return true;
				}
				else
				{
					TShock.Log.ConsoleError("DieMob config not found. Creating new one");
					CreateConfig();
					return false;
				}
			}
			catch (Exception ex)
			{
				TShock.Log.ConsoleError(ex.Message);
			}
			return false;
		}


		#region Hooks
		private void OnRegionDelete(RegionHooks.RegionDeletedEventArgs args)
		{
			if (Database.DieMobRegions.Exists(p => p.TSRegion.Name == args.Region.Name))
			{
				Database.DieMob_Delete(args.Region.Name);
			}
		}
		void OnInitialize(EventArgs e)
		{
			if (!Directory.Exists(savepath))
			{
				Directory.CreateDirectory(savepath);
				CreateConfig();
			}
			ReadConfig();
			Commands.ChatCommands.Add(new Command("diemob", PluginCommands.DieMobCommand, "diemob", "DieMob", "dm"));
			Database.Connect();
		}
		private static void OnWorldLoad()
		{
			Database.DieMob_Read();
		}
		private void OnUpdate(EventArgs e)
		{
			if ((DateTime.UtcNow - lastUpdate).TotalMilliseconds >= config.UpdateInterval)
			{
				lastUpdate = DateTime.UtcNow;
				if (!initialized && Main.worldID > 0)
				{
					initialized = true;
					OnWorldLoad();
				}
				try
				{
					for (int r = 0; r < Database.DieMobRegions.Count; r++)
					{
						Region reg = TShock.Regions.GetRegionByName(Database.DieMobRegions[r].TSRegion.Name);
						if (reg == null)
						{
							Database.DieMob_Delete(Database.DieMobRegions[r].TSRegion.Name);
							continue;
						}
						DieMobRegion Region = Database.DieMobRegions[r];
						Region.TSRegion = reg;
						for (int i = 0; i < Main.npc.Length; i++)
						{
							if (Main.npc[i].active)
							{
								NPC npc = Main.npc[i];
								if ((npc.friendly && Region.AffectFriendlyNPCs && npc.netID != 488) ||
									(!npc.friendly && npc.SpawnedFromStatue && Region.AffectStatueSpawns && npc.netID != 488 && npc.catchItem == 0) ||
									(!npc.friendly && !npc.SpawnedFromStatue && npc.netID != 488 && npc.catchItem == 0))
								{
									if (Region.TSRegion.InArea((int)(Main.npc[i].position.X / 16), (int)(Main.npc[i].position.Y / 16)))
									{
										if (Region.ReplaceMobs.ContainsKey(npc.netID))
										{
											npc.SetDefaults(Region.ReplaceMobs[npc.netID]);
											NetMessage.SendData((int)PacketTypes.NpcUpdate, -1, -1, NetworkText.Empty, i);
										}
										else if (Region.ReplaceMobs.ContainsKey(-100))
										{
											npc.SetDefaults(Region.ReplaceMobs[-100]);
											NetMessage.SendData((int)PacketTypes.NpcUpdate, -1, -1, NetworkText.Empty, i);
										}
										else if (Region.Type == RegionType.Repel)
										{
											Rectangle area = Region.TSRegion.Area;
											int yDir = -10;
											if (area.Bottom - (int)(npc.position.Y / 16) < area.Height / 2)
												yDir = 10;
											int xDir = -10;
											if (area.Right - (int)(npc.position.X / 16) < area.Width / 2)
												xDir = 10;
											npc.velocity = new Vector2(xDir * config.RepelPowerModifier, yDir * config.RepelPowerModifier);
											NetMessage.SendData((int)PacketTypes.NpcUpdate, -1, -1, NetworkText.Empty, i);
										}
										else if (Region.Type == RegionType.Kill)
										{
											Main.npc[i] = new NPC();
											NetMessage.SendData((int)PacketTypes.NpcUpdate, -1, -1, NetworkText.Empty, i);
										}
									}
								}
							}
						}
					}

				}
				catch (Exception ex)
				{
					TShock.Log.ConsoleError(ex.ToString());
				}
			}
		}
		#endregion

	}
		
}
