using DiscGolfCourseApi.Exceptions;
using DiscGolfCourseApi.Models;
using NetTopologySuite.Features;
using NetTopologySuite.IO;

namespace DiscGolfCourseApi.Repositories;

public class GeographyRepository : Repository
{
    private const string State = "STATE";
    private const string County = "COUNTIES";
    private const string Zipcode = "ZIPCODE";
    
    public GeographyRepository(DiscgolfDbContext db) : base(db)
    {
    }

    #region public
    
    /// <summary>
    /// gets a geography for a given id
    /// </summary>
    /// <param name="geoId">the id to find</param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public UsGeography GetGeographyForId(long geoId)
    {
        var geo = Db.UsGeographies.FirstOrDefault(x => x.GeoId == geoId) 
                  ?? throw new NotFoundException($"Could not find geography for id {geoId}");

        geo.LatLngs = GetLatLngs(geo.Boundary);
        return geo;
    }
    
    /// <summary>
    /// gets all US states, optionally including the boundary 
    /// </summary>
    /// <param name="includeGeographies"></param>
    /// <returns></returns>
    public List<UsGeography> GetStates(bool includeGeographies)
    {
        var states = Db.UsGeographies.Where(x => x.GeoType == State);
        if (includeGeographies)
        {
            foreach (var state in states)
            {
                state.LatLngs = GetLatLngs(state.Boundary);
            }    
        }
        
        return states
            .OrderBy(x => x.GeoName)
            .ToList();
    }

    public List<UsGeography> GetCountiesForStates(List<long> stateIds)
    {
        var countyIds = Db.UsGeographyRelationships
            .Where(x => stateIds.Contains(x.ParentGeoId))
            .Select(x => x.GeoId);
        
        var counties = Db.UsGeographies
            .Where(x => countyIds.Contains(x.GeoId))
            .OrderBy(x => x.GeoName);
        
        return counties.ToList();
    } 

    #endregion

    #region private
    
    /// <summary>
    /// </summary>
    /// <param name="path"></param>
    public async Task ReadGeoJsonCounties(string path, string statePath)
    {
        var geoJsonReader = new GeoJsonReader();
        List<UsGeographyRelationships> relationships = new();
        List<UsGeography> newCounties = new();

        var namesForStateNum = await ReadGeoJsonStates(statePath);

        using var reader = new StreamReader(path);

        var raw = await reader.ReadToEndAsync();
        var featureCollection = geoJsonReader.Read<FeatureCollection>(raw);

        var states = Db.StateAbbreviations.AsEnumerable();
        var counties = Db.UsGeographies.Where(x => x.GeoType == "COUNTY").AsEnumerable();

        foreach (var feature in featureCollection)
        {
            var stateNum = int.Parse(feature.Attributes["STATE"].ToString());
            var name = (string) feature.Attributes["NAME"];

            var geography = feature.Geometry.Buffer(0).Reverse();
            
            var stateAbbrev = states.FirstOrDefault(x => x.FullName == namesForStateNum[stateNum]);
            if (stateAbbrev == null)
            {
                throw new Exception($"Could not find {stateAbbrev}");
            }
            
            var newCounty = new UsGeography
            {
                GeoName = name,
                GeoType = "COUNTY",
                Boundary = geography
            };

            Db.UsGeographies.Add(newCounty);
            await Db.SaveChangesAsync();
            
            relationships.Add(new UsGeographyRelationships
            {
                GeoName = name,
                GeoId = newCounty.GeoId,
                ParentGeoName = stateAbbrev.FullName,
                ParentGeoId = stateAbbrev.StateNumber ?? -1,
            });
        }
        
        Console.WriteLine("Starting to load geographies...");
        // Db.UsGeographies.AddRange(counties);
        // await Db.SaveChangesAsync();
        Console.WriteLine("Finished loading geographies!");
        
        Console.WriteLine("Starting to load relationships...");
        Db.UsGeographyRelationships.AddRange(relationships);
        await Db.SaveChangesAsync();
        Console.WriteLine("Finished loading relationships!");
    }
    
    /// <summary>
    /// </summary>
    /// <param name="path"></param>
    private async Task<Dictionary<int, string>> ReadGeoJsonStates(string path)
    {
        var geoJsonReader = new GeoJsonReader();

        using var reader = new StreamReader(path);

        var result = new Dictionary<int, string>();

        var raw = await reader.ReadToEndAsync();
        var featureCollection = geoJsonReader.Read<FeatureCollection>(raw);

        foreach (var feature in featureCollection)
        {
            var num = int.Parse(feature.Attributes["STATE"].ToString());
            var name = (string) feature.Attributes["NAME"];
            result.Add(num, name);
        }

        return result;
    }

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