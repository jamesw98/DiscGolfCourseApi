using DiscGolfCourseApi.Exceptions;
using DiscGolfCourseApi.Models;
using DiscGolfCourseApi.Models.Requests;
using DiscGolfCourseApi.Repositories;
using DiscGolfCourseApi.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DiscGolfCourseApi.Controllers;

[ApiController]
[Route("api/course")]
public class CourseController: Controller<CourseRepository>
{
    private readonly PdgaCourseScraper _scraper;
    
    public CourseController(CourseRepository repo, PdgaCourseScraper scraper) : base(repo)
    {
        _scraper = scraper;
    }

    [HttpGet]
    [SwaggerOperation("Gets all courses in the database")]
    [SwaggerResponse(200, "Found courses", typeof(List<Course>))]
    [SwaggerResponse(404, "No courses found")]
    public IActionResult GetCourses()
    {
        // get the courses and return them
        var courses = Repo.GetCourses();
        return courses.Any()
            ? Ok(courses)
            : NotFound();
    }

    [HttpGet]
    [Route("scrape-many")]
    [SwaggerOperation("Scrapes multiple courses given a url of a course list page")]
    public async Task<IActionResult> ScrapeMany([FromBody] CreateCourseRequest request)
    {
        var existingCourses = Repo.GetExistingCourseUrlNames();
        
        var (newCourses, errors) = await _scraper
            .GetCourseNamesForPage(request.Url, existingCourses);
        
        await Repo.CreateMultipleCourses(newCourses);
        return Ok(newCourses);
    }
    
    [HttpPut]
    [Route("fix-missing-data")]
    [SwaggerOperation("Fixes missing NTS point, state, and zipcode data for any records in the db")]
    public async Task<IActionResult> FixCourses()
    {
        try
        {
            await Repo.FixNullPoints();
            return Ok();
        }
        catch (ScraperException se)
        {
            return BadRequest(se.Message);
        }
    }

    [HttpGet]
    [Route("{courseId}")]
    public IActionResult GetCourse([FromRoute] long courseId)
    {
        var course = Repo.GetCourse(courseId);
        return course != null
            ? Ok(course)
            : NotFound();
    }

    [HttpPost]
    [Route("scrape")]
    [SwaggerOperation("Puts in a request to scrape a course an insert it into the database")]
    [SwaggerResponse(200, "Course was created successfully", typeof(Course))]
    [SwaggerResponse(404, "Could not find the course that was requested")]
    [SwaggerResponse(400, "The course already exists/there was an error parsing the course details")]
    public async Task<IActionResult> AddSingleCourse([FromBody] CreateCourseRequest request)
    {
        // ensure this course doesn't already exist
        if (Repo.CourseExists(request.Url))
        {
            return BadRequest("This course already exists in the database");
        }

        try
        {
            // scrape the course from the pdga site
            var newCourse = await _scraper.ScrapeSingleCourse(request.Url.ToLower());
            // insert it into to the database
            return Ok(await Repo.CreateCourse(newCourse));
        }
        catch (ScraperException se)
        {
            return BadRequest(se.Message);
        }
        catch (NotFoundException nfe)
        {
            return NotFound(nfe.Message);
        }
    }
}