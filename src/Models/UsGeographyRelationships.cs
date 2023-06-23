namespace DiscGolfCourseApi.Models;

public class UsGeographyRelationships
{
    public long RelationshipId { get; set; }
    public required string GeoName { get; set; }
    public required string ParentGeoName { get; set; }
    public long GeoId { get; set; }
    public long ParentGeoId { get; set; }
}