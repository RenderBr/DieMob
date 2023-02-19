using Auxiliary;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using TShockAPI;

namespace DieMob.Api
{
    public class DiemobApi
    {

        public void DeleteDiemob(string regionName)
        {
            StorageProvider.GetMongoCollection<DieMobRegion>("DieMobRegions").FindOneAndDeleteAsync(x => x.Region == regionName);
        }

        public List<DieMobRegion> RetrieveAllRegions()
        {
            return StorageProvider.GetMongoCollection<DieMobRegion>("DieMobRegions").Find(x=>true).ToList();

        }

        public async Task<DieMobRegion> RetrieveRegion(string regionName)
        {
            return await IModel.GetAsync(GetRequest.Bson<DieMobRegion>(x => x.Region == regionName));
        }


        public async Task<DieMobRegion> CreateDieMobRegion(string regionName)
            => await IModel.GetAsync(GetRequest.Bson<DieMobRegion>(x => x.Region == regionName), x=>
            {
                x.Region = TShock.Regions.GetRegionByName(regionName).Name;
                x.AffectFriendlyNPCs = false;
                x.AffectStatueSpawns = true;
                x.ReplaceMobs = new();
                x.Type = RegionType.Kill;
            });

       
    }
}
