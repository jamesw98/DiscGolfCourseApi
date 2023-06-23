using DiscGolfCourseApi.Models;
using GoogleMaps.LocationServices;

namespace DiscGolfCourseApi.Repositories;

public class CourseRepository : Repository
{
    private IConfiguration _config;
    
    public CourseRepository(IConfiguration config, DiscgolfDbContext db) : base(db)
    {
        _config = config;
    }

    #region public
    
    /// <summary>
    /// gets a course for a specific id
    /// </summary>
    /// <param name="courseId"></param>
    /// <returns></returns>
    public Course? GetCourse(long courseId)
    {
        return Db.Courses.FirstOrDefault(x => x.CourseId == courseId);
    }   
    
    /// <summary>
    /// gets a dictionary of course ids for the corresponding course name
    /// this will be used on the front end instead of loading all courses on page load, once a user selects a course
    /// or courses to display on the map
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, long> GetIdsForCourseNames()
    {
        return Db.Courses.ToDictionary(x => x.CourseName, x => x.CourseId);
    }
    
    /// <summary>
    /// gets all courses or all courses within a state
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    ///     if point data wasn't found for a course, in theory this won't happen
    /// </exception>
    public List<Course> GetCourses()
    {
        var courses = Db.Courses;
        // this gets around funky serializing issues with NTS
        foreach (var c in courses)
        {
            // grab the wkt, this is essentially STAsText()
            c.PointWkt = c.Point?.AsText() 
                         ?? throw new ArgumentException($"No point found for {c.CourseName}");
        }

        return courses.ToList();
    }

    public async Task FixNullPoints()
    {
        var coursesWithNullPoints = Db.Courses
            .Where(x => x.Point == null)
            .AsEnumerable();

        foreach (var course in coursesWithNullPoints)
        {
            var locationService = new GoogleLocationService(_config["GoogleLocationServiceApiKey"]);

            // get the point from google maps services, then convert it to an NTS point and wkt string        
            var point = locationService.GetLatLongFromAddress(course.CourseAddress);
            course.Latitude = point.Latitude;
            course.Longitude = point.Longitude;
            course.Point = new NetTopologySuite.Geometries.Point(point.Longitude, point.Latitude) { SRID = 4326 }; 
            course.PointWkt = course.Point.AsText();
        }

        await Db.SaveChangesAsync();
    }

    /// <summary>
    /// creates a course to the database
    /// </summary>
    /// <param name="course">the course to create</param>
    /// <returns></returns>
    public async Task<Course> CreateCourse(Course course)
    {
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();
        return course;
    }
    
    /// <summary>
    /// creates multiple courses
    /// </summary>
    /// <param name="courses">pass by reference, the courses to create</param>
    /// <returns></returns>
    public async Task CreateMultipleCourses(List<Course> courses)
    {
        foreach (var course in courses)
        {
            Db.Courses.Add(course);
        }

        await Db.SaveChangesAsync();
    }
    
    /// <summary>
    /// checks if a course is already in the database
    /// </summary>
    /// <param name="rawCourseName"></param>
    /// <returns></returns>
    public bool CourseExists(string rawCourseName)
    {
        return Db.Courses.Any(x => x.RawCourseName == rawCourseName);
    }
    
    /// <summary>
    /// gets all the course url names in the database
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string?> GetExistingCourseUrlNames()
    {
        return Db.Courses.Select(x => x.RawCourseName);
    }
    
    /// <summary>
    /// finds all courses within a given zipcode
    /// optionally returns a list of lats and lons that represents the given zipcode
    /// </summary>
    /// <param name="zipcode"></param>
    /// <param name="includeGeo"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">if the given zipcode is not valid</exception>
    public CoursesAndGeography GetCoursesInZipcode(string zipcode, bool includeGeo)
    {
        var zipcodeGeo = Db.UsGeographies.SingleOrDefault(x => x.GeoName == zipcode);
        if (zipcodeGeo == null)
        {
            throw new ArgumentException($"Could not found {zipcode}");
        }

        var courses = Db.Courses.Where(x => x.Point != null && x.Point.Within(zipcodeGeo.Boundary));

        if (includeGeo)
        {
            zipcodeGeo.LatLngs = GetLatLngs(zipcodeGeo.Boundary);
        }

        return new CoursesAndGeography
        {
            Courses = courses.ToList(),
            Geography = zipcodeGeo.LatLngs
        };
    }

    public CoursesAndGeographies FindCourses(List<long> geoIds)
    {
        var geographies = Db.UsGeographies
            .Where(x => geoIds.Contains(x.GeoId)).ToList();

        List<Course> courses = new();

        foreach (var geo in geographies)
        {
            var coursesWithinGeo = Db.Courses
                .Where(x => x.Point != null && x.Point.Within(geo.Boundary));
            courses.AddRange(coursesWithinGeo);
            geo.LatLngs = GetLatLngs(geo.Boundary);
        }

        return new CoursesAndGeographies
        {
            Courses = courses,
            Geographies = geographies
        };
    }

    #endregion

    #region private

    

    #endregion
}