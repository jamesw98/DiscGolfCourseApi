using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using FisSst.BlazorMaps;
using NetTopologySuite.Geometries;

namespace DiscGolfCourseApi.Models;

public class UsGeography
{
    public string GeoName { get; set; }
    public string GeoType { get; set; }
    [JsonIgnore]
    public Geometry Boundary { get; set; }
    [NotMapped]
    public List<LatLng> LatLngs { get; set; }
}