using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using NetTopologySuite.Geometries;

namespace DiscGolfCourseApi.Models;

public class Course
{
    /// <summary>
    /// auto-increment course id
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long CourseId { get; set; }
    /// <summary>
    /// name of the course
    /// </summary>
    public string CourseName { get; set; } = null!;
    /// <summary>
    /// how many holes the course has
    /// </summary>
    public int HoleCount { get; set; }
    /// <summary>
    /// the type of basket used (Black Hole, DISCatcher, Veteran, etc)
    /// </summary>
    public string? TargetType { get; set; }
    /// <summary>
    /// tee type (turf, concrete, dirt, etc)
    /// </summary>
    public string? TeeType { get; set; }
    /// <summary>
    /// count of holes under 300ft
    /// </summary>
    public int ShortHoleCount { get; set; }
    /// <summary>
    /// count of holes between 300 and 400ft
    /// </summary>
    public int MediumHoles { get; set; }
    /// <summary>
    /// count of holes over 400ft
    /// </summary>
    public int LongHoles { get; set; }
    /// <summary>
    /// description of the course
    /// </summary>
    public string CourseDesc { get; set; } = null!;
    /// <summary>
    /// directions to the course
    /// </summary>
    public string? Directions { get; set; }
    /// <summary>
    /// location type (park, private, church, etc)
    /// </summary>
    public string LocationType { get; set; } = null!;
    /// <summary>
    /// course type (permanent, temporary, etc)
    /// </summary>
    public int CourseType { get; set; }
    /// <summary>
    /// address of the course
    /// </summary>
    public string? CourseAddress { get; set; }
    /// <summary>
    /// average rating of the course out of 5 
    /// </summary>
    public double CourseRating { get; set; }
    /// <summary>
    /// whether or not this data is the latest for this course in the database
    /// </summary>
    public bool Latest { get; set; }
    /// <summary>
    /// the date this record was created
    /// </summary>
    public DateTime CreatedDate { get; set; }
    /// <summary>
    /// a NTS point, used for geographic calculations
    /// </summary>
    [JsonIgnore]
    public Point? Point { get; set; }
    /// <summary>
    /// the raw course name from the pdga site (alexander-park, east-roswell-park, chamblee-church, etc)
    /// </summary>
    public string? RawCourseName { get; set; }
    /// <summary>
    /// latitude for the address of the course
    /// </summary>
    public double Latitude { get; set; }
    /// <summary>
    /// longitude for the address of the course
    /// </summary>
    public double Longitude { get; set; }
    /// <summary>
    /// wkt string representation of Point, this not stored in the db
    /// </summary>
    [NotMapped]
    public string? PointWkt { get; set; }
}
