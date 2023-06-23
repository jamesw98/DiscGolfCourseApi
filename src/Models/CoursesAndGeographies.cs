namespace DiscGolfCourseApi.Models;

public class CoursesAndGeographies
{
    public required List<Course> Courses { get; set; }
    public required List<UsGeography> Geographies { get; set; }
}