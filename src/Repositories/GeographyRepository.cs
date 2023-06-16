using DiscGolfCourseApi.Models;
using NetTopologySuite.Features;
using NetTopologySuite.IO;

namespace DiscGolfCourseApi.Repositories;

public class GeographyRepository : Repository
{
    public GeographyRepository(DiscgolfDbContext db) : base(db)
    {
    }
    
    #region private
    
    /// <summary>
    /// this can be made public whenever zipcodes need to be updated, but this shouldn't need to be run fairly often
    /// this loads zipcodes into the database (TBL_US_GEOGRAPHIES) 
    /// </summary>
    /// <param name="path"></param>
    private async Task ReadGeoJsonZips(string path)
    {
        var geoJsonReader = new GeoJsonReader();
        List<UsGeography> zips = new();
        
        using var reader = new StreamReader(path);
        
        var line = await reader.ReadLineAsync();
        while (line != null)
        {
            var feature = geoJsonReader.Read<Feature>(line);
            var name = feature.Attributes["ZIP_CODE"].ToString();
            // why do i need to call these 2 methods? i don't know! but if i don't, entity framework yells at me!
            // if i don't do this, entity framework is angry saying that geographies must be counter clockwise
            var geography = feature.Geometry.Buffer(0).Reverse();
            
            if (geography == null || name == null)
            {
                continue;
            }

            zips.Add(new UsGeography
            {
                GeoName = name,
                GeoType = "ZIPCODE",
                Boundary = geography
            });
            
            // hack! hack! evil! bad! stupid!
            if (name == "99950")
            {
                break;
            }
            
            line = await reader.ReadLineAsync();
        }
        
        Console.WriteLine("Starting to load geographies...");
        await Db.UsGeographies.AddRangeAsync(zips);
        await Db.SaveChangesAsync();
        Console.WriteLine("Finished loading geographies!");
    }

    #endregion
}