using DiscGolfCourseApi.Models;

namespace DiscGolfCourseApi.Repositories;

public class DetailsRepository : Repository
{
    public DetailsRepository(DiscgolfDbContext db) : base(db)
    {
    }

    public List<string> GetZipcodes()
    {
        return Db.UsGeographies
            .Where(x => x.GeoType == "ZIPCODE")
            .Select(x => x.GeoName)
            .ToList();
    }
    
    public FilterDetails GetFilterDetails(bool includeZipcodes)
    {
        return new FilterDetails
        {
            Zipcodes = includeZipcodes 
                ? Db.UsGeographies
                    .Where(x => x.GeoType == "ZIPCODE")
                    .Select(x => x.GeoName)
                    .ToList()
                : null,
            TargetTypes = Db.Courses
                .Select(x => x.TargetType)
                .Distinct()
                .ToList(),
            TeeTypes = Db.Courses
                .Select(x => x.TeeType)
                .Distinct()
                .ToList(),
            LocationType = Db.Courses
                .Select(x => x.LocationType)
                .Distinct()
                .ToList(),
            HoleCounts = Db.Courses
                .Select(x => x.HoleCount)
                .Distinct()
                .ToList()
        };
    }
}