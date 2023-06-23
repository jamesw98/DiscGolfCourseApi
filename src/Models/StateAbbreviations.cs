namespace DiscGolfCourseApi.Models;

public class StateAbbreviations
{
    public required string FullName { get; set; }
    public required string Abbreviation { get; set; }
    public long? StateNumber { get; set; }
}