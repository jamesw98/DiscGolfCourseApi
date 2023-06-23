namespace DiscGolfCourseApi.Models;

public class FilterDetails
{
    public List<string>? Zipcodes { get; set; }
    public required List<string?> TargetTypes { get; set; }
    public required List<string?> TeeTypes { get; set; }
    public required List<string> LocationType { get; set; }
    public required List<int> HoleCounts { get; set; }
}