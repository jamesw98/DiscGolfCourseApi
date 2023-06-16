namespace DiscGolfCourseApi.Models.Requests;

public class CreateCourseRequest
{
    /// <summary>
    /// the name at the end of the pdga course url:
    /// ex:
    ///     chattahoochee-pointe-disc-golf-course-0
    ///     etowah
    ///     east-roswell-park
    /// </summary>
    public required string Url { get; set; } 
}