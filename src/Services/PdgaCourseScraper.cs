using DiscGolfCourseApi.Exceptions;
using DiscGolfCourseApi.Models;
using DiscGolfCourseApi.Models.Enums;
using GoogleMaps.LocationServices;
using HtmlAgilityPack;
using NetTopologySuite.Geometries;

namespace DiscGolfCourseApi.Services;

public class PdgaCourseScraper
{
    private IConfiguration _config;
    private readonly HttpClient _client;
    private const string BaseUrl = "https://www.pdga.com/course-directory/course/";
    private const string MapPageTitle = "PDGA Disc Golf Course Directory Map | Professional Disc Golf Association";

    public PdgaCourseScraper(IConfiguration config, HttpClient client)
    {
        _config = config;
        _client = client;
    }

    #region public

    /// <summary>
    /// gets all course names for a given url, these names can be used for the ScrapeSingle and ScrapeMultiple methods
    /// </summary>
    /// <param name="url"></param>
    /// <param name="existingNames"></param>
    /// <returns></returns>
    public async Task<(List<Course>, Dictionary<string, string>)> GetCourseNamesForPage
    (
        string url,
        IEnumerable<string?> existingNames
    )
    {
        var page = await _client.GetStringAsync(url);
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(page);

        var mainList = htmlDoc.DocumentNode.SelectNodes("//div")
            .FirstOrDefault(x => x.HasClass("view-course-directory")) 
                       ?? throw new ScraperException("Could not find course dir div");
        
        var names = mainList.SelectNodes("//a[@href]")
            .Where(x => x.GetAttributeValue("href", string.Empty).Contains("course-directory/course"))
            .Select(x => x.GetAttributeValue("href", string.Empty).Split("/")[3]);
        
        return await ScrapeMultipleCourses(names, existingNames);
    }

    /// <summary>
    /// scrape multiple courses and put them in the database
    /// </summary>
    /// <param name="courseUrlNames"></param>
    /// <param name="existingNames"></param>
    /// <returns></returns>
    public async Task<(List<Course>, Dictionary<string, string>)> ScrapeMultipleCourses
    (
        IEnumerable<string> courseUrlNames,
        IEnumerable<string?> existingNames
    )
    {
        Dictionary<string, string> reasonsForErroredCourses = new();
        List<Course> newCourseIds = new();

        var newCourses = courseUrlNames.Where(x => !existingNames.Contains(x));
        foreach (var courseName in newCourses)
        {
            try
            {
                Console.WriteLine(courseName);
                var newCourse = await ScrapeSingleCourse(courseName);
                newCourseIds.Add(newCourse);
            }
            catch (Exception e)
            {
                reasonsForErroredCourses.Add(courseName, e.Message);
            }
        }

        return (newCourseIds, reasonsForErroredCourses);
    }

    /// <summary>
    /// scrapes a single course page and returns a course object
    /// </summary>
    /// <param name="url">url for the course</param>
    /// <returns>a filled out course object</returns>
    public async Task<Course> ScrapeSingleCourse(string url)
    {
        var locationService = new GoogleLocationService(_config["GoogleLocationServiceApiKey"]);
        
        var courseDetailsRaw = await _client.GetStringAsync(url);
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(courseDetailsRaw);
        
        /*
         * if the page title is the map title, that means we've been redirected to the map page, which means the course
         * requested does not exist
         */
        if (htmlDoc.DocumentNode.SelectNodes("//title").FirstOrDefault()?.InnerHtml == MapPageTitle)
        {
            throw new NotFoundException($"Could not find course {url}");
        }
        
        // create the base course object
        var resultCourse = new Course
        {
            RawCourseName = url.Split("/")[3],
            CreatedDate = DateTime.Now,
            Latest = true
        };
        
        // get the course name
        PopulateCourseName(htmlDoc, resultCourse);
        
        // get the "main block"
        var mainBlock = GetMainBlock(htmlDoc);
            
        // get details
        PopulateCourseType(mainBlock, resultCourse);
        PopulateCourseRating(mainBlock, resultCourse);
        PopulateLocationType(mainBlock, resultCourse);
        PopulateAddress(mainBlock, resultCourse);
        PopulateHoleCount(mainBlock, resultCourse);
        PopulateTargetType(mainBlock, resultCourse);
        PopulateTeeType(mainBlock, resultCourse);
        PopulateDescription(mainBlock, resultCourse);
        PopulateDirections(mainBlock, resultCourse);

        // get the point from google maps services, then convert it to an NTS point and wkt string        
        var point = locationService.GetLatLongFromAddress(resultCourse.CourseAddress);
        resultCourse.Latitude = point.Latitude;
        resultCourse.Longitude = point.Longitude;
        resultCourse.Point = new Point(point.Longitude, point.Latitude) { SRID = 4326 }; 
        resultCourse.PointWkt = resultCourse.Point.AsText();

        return resultCourse;
    }

    #endregion

    #region private

    /// <summary>
    /// gets the main block for the course page
    /// </summary>
    /// <param name="htmlDoc">the base html page</param>
    /// <returns></returns>
    /// <exception cref="ScraperException"></exception>
    private static HtmlNode GetMainBlock(HtmlDocument htmlDoc)
    {
        return htmlDoc.DocumentNode.SelectNodes("//div")
                   .FirstOrDefault(x => x.Id == "block-system-main") 
               ?? throw new ScraperException("Could not find main block");
    }
    
    /// <summary>
    /// populates the course name field of a course object
    /// </summary>
    /// <param name="htmlDoc">the html doc to scrape</param>
    /// <param name="course">passed by ref</param>
    /// <exception cref="ScraperException"></exception>
    private static void PopulateCourseName(HtmlDocument htmlDoc, Course course)
    {
        course.CourseName = htmlDoc.DocumentNode.SelectNodes("//h1")
                .FirstOrDefault()?.ChildNodes
                .FirstOrDefault()?.InnerHtml
            ?? throw new ScraperException("Could not find course name");
    }
    
    /// <summary>
    /// populates the course type file of a course object
    /// </summary>
    /// <param name="mainBlock">the main block of html to scrape</param>
    /// <param name="course">passed by ref</param>
    /// <exception cref="ScraperException"></exception>
    private static void PopulateCourseType(HtmlNode mainBlock, Course course)
    {
        // get the course type as a string
        var strCourseType = mainBlock.SelectNodes("//div")
                                .FirstOrDefault(x => x.HasClass("field-name-field-course-type"))?.InnerText
                                .Split("&nbsp;")[1]
                            ?? "Unknown";
        
        // try to convert the course type to an enum
        if (!Enum.TryParse<CourseType>(strCourseType, out var enumCourseType))
        {
            throw new ScraperException($"Unknown course type: {strCourseType}");
        }
        course.CourseType = (int) enumCourseType;
    }   
    
    /// <summary>
    /// populates the course rating field of a course object
    /// </summary>
    /// <param name="mainBlock"></param>
    /// <param name="course"></param>
    /// <exception cref="ScraperException"></exception>
    private static void PopulateCourseRating(HtmlNode mainBlock, Course course)
    {
        // get the course rating as a string
        var strAverageRating = mainBlock.SelectNodes("//span")
                                   .FirstOrDefault(x => x.HasClass("average-rating"))?.InnerText
                                   .Split(": ")[1] 
                               ?? "0";
        
        // try to convert the course rating to a double 
        if (!double.TryParse(strAverageRating, out var averageRating))
        {
            throw new ScraperException($"Could not parse course rating: {strAverageRating}");
        }
        course.CourseRating = averageRating;
    }
    
    /// <summary>
    /// populates the location type field of a course object
    /// </summary>
    /// <param name="mainBlock"></param>
    /// <param name="course"></param>
    /// <exception cref="ScraperException"></exception>
    private static void PopulateLocationType(HtmlNode mainBlock, Course course)
    {
        // get the location type
        course.LocationType = mainBlock.SelectNodes("//div")
                                  .FirstOrDefault(x => x.HasClass("field-name-field-location-type"))?.InnerText
                                  .Split("&nbsp;")[1]
                              ?? "Unknown";
    }
    
    /// <summary>
    /// does what it says it does
    /// </summary>
    /// <param name="mainBlock"></param>
    /// <param name="course"></param>
    private static void PopulateAddress(HtmlNode mainBlock, Course course)
    {
        course.CourseAddress = mainBlock.SelectNodes("//div")
            .FirstOrDefault(x => x.HasClass("field-name-field-course-location"))?.InnerText 
                               ?? throw new ScraperException("Could not find address");
    }
    
    /// <summary>
    /// does what it says it does
    /// </summary>
    /// <param name="mainBlock"></param>
    /// <param name="course"></param>
    /// <exception cref="ScraperException"></exception>
    private static void PopulateHoleCount(HtmlNode mainBlock, Course course)
    {
        var strHoleCount = mainBlock.SelectNodes("//div")
            .FirstOrDefault(x => x.HasClass("field-name-field-course-holes"))?.InnerText
            .Split("&nbsp;")[1] 
                           ?? "0";

        if (!int.TryParse(strHoleCount, out var holeCount))
        {
            throw new ScraperException($"Could not parse course hole count: {strHoleCount}");
        }
        course.HoleCount = holeCount;
    }
    
    /// <summary>
    /// does what it says it does
    /// </summary>
    /// <param name="mainBlock"></param>
    /// <param name="course"></param>
    /// <exception cref="ScraperException"></exception>
    private static void PopulateTargetType(HtmlNode mainBlock, Course course)
    {
        course.TargetType = mainBlock.SelectNodes("//div")
            .FirstOrDefault(x => x.HasClass("views-field-field-course-target-type-revision-id"))?.InnerText
            .Split(": ")[1]
                           ?? "Unknown";
    }
    
    /// <summary>
    /// does what it says it does
    /// </summary>
    /// <param name="mainBlock"></param>
    /// <param name="course"></param>
    /// <exception cref="ScraperException"></exception>
    private static void PopulateTeeType(HtmlNode mainBlock, Course course)
    {
        course.TeeType = mainBlock.SelectNodes("//div")
            .FirstOrDefault(x => x.HasClass("views-field-field-course-tee-type-revision-id"))?.InnerText
            .Split(": ")[1]
                            ?? "Unknown";
    }
    
    /// <summary>
    /// does what it says it does
    /// </summary>
    /// <param name="mainBlock"></param>
    /// <param name="course"></param>
    /// <exception cref="ScraperException"></exception>
    private static void PopulateDescription(HtmlNode mainBlock, Course course)
    {
        course.CourseDesc = mainBlock.SelectNodes("//div")
            .FirstOrDefault(x => x.HasClass("views-field-nothing-3"))?.NextSibling.InnerText 
                   ?? "No description on PDGA site";
    }
    
    /// <summary>
    /// does what it says it does
    /// </summary>
    /// <param name="mainBlock"></param>
    /// <param name="course"></param>
    /// <exception cref="ScraperException"></exception>
    private static void PopulateDirections(HtmlNode mainBlock, Course course)
    {
        course.Directions = mainBlock.SelectNodes("//div")
            .FirstOrDefault(x => x.HasClass("views-field-field-course-directions-revision-id"))?.InnerText 
                            ?? "Unknown";
    }

    #endregion
}