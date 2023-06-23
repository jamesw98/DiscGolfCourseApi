using DiscGolfCourseApi.Models;
using FisSst.BlazorMaps;
using NetTopologySuite.Geometries;

namespace DiscGolfCourseApi.Repositories;

public class Repository
{
    protected DiscgolfDbContext Db;

    public Repository(DiscgolfDbContext db)
    {
        Db = db;
    }
    
    protected static List<LatLng> GetLatLngs(Geometry geo)
    {
        return geo.Coordinates.Select(coord => new LatLng(coord.Y, coord.X)).ToList();
    }
}