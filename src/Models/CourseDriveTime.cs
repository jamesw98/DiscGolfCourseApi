using NetTopologySuite.Geometries;

namespace DiscGolfCourseApi.Models;

public class CourseDriveTime
{
    public long DrivetimeId { get; set; }

    public long CourseId { get; set; }

    public int? DrivetimeMinutes { get; set; }

    public Course Course { get; set; } = null!;
    
    public required Geometry Geometry { get; set; }
}
