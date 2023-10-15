using Auxiliary.Configuration;
using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TShockAPI;

namespace DieMob.Api
{
	public class DiemobApi
	{
        public string dbPath = Path.Combine(TShock.SavePath, "diemob.sqlite");
        public SQLiteAsyncConnection SQL;
		public async Task DeleteDiemob(string regionName) => await SQL.Table<DieMobRegion>().DeleteAsync(x=>x.Region==regionName);

		public async Task<List<DieMobRegion>> RetrieveAllRegions() => await SQL.Table<DieMobRegion>().ToListAsync();

		public async Task<DieMobRegion>? RetrieveRegion(string regionName) => await SQL.Table<DieMobRegion>().FirstOrDefaultAsync(x => x.Region == regionName);


		public async Task CreateDieMobRegion(string regionName)
			=> await SQL.InsertAsync(new DieMobRegion
			{
				Region = regionName,
				Type = RegionType.Kill
			});

		public void ReloadDieMob() => Configuration<DiemobSettings>.Load(nameof(DieMob));


	}
}
