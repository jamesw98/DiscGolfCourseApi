using FisSst.BlazorMaps;

namespace DiscGolfCourseApi.Models;

public class CoursesAndGeography
{
    public required List<Course> Courses { get; set; }
    public List<LatLng>? Geography { get; set; }
}