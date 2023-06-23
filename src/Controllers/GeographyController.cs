using DiscGolfCourseApi.Exceptions;
using DiscGolfCourseApi.Models;
using DiscGolfCourseApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DiscGolfCourseApi.Controllers;

[Route("api/geography")]
public class GeographyController : Controller<GeographyRepository>
{
    public GeographyController(GeographyRepository repo) : base(repo)
    {
    }

    [HttpPost]
    [Route("states/counties")]
    [SwaggerOperation("Gets counties that lie within the provided states")]
    public IActionResult GetCountiesInStates([FromBody] List<long> stateIds)
    {
        try
        {
            return Ok(Repo.GetCountiesForStates(stateIds));
        }
        catch (Exception e)
        {
            return Problem(e.Message);
        }
    }
    
    [HttpGet]
    [Route("states")]
    [SwaggerOperation("Gets all US states, optionally including the boundary")]
    public IActionResult GetStates([FromQuery] bool includeBoundary)
    {
        try
        {
            return Ok(Repo.GetStates(includeBoundary));
        }
        catch (Exception e)
        {
            return Problem(e.Message);
        }
    }
    
    [HttpGet]
    [Route("{geographyId}")]
    [SwaggerOperation("Gets a geography for a given id")]
    [SwaggerResponse(200, "Successfully found a geography", typeof(UsGeography))]
    [SwaggerResponse(404, "Could not find a geography for the given id")]
    public IActionResult GetGeographyForId([FromRoute] long geographyId)
    {
        try
        {
            return Ok(Repo.GetGeographyForId(geographyId));
        }
        catch (NotFoundException nfe)
        {
            return NotFound(nfe.Message);
        }
        catch (Exception e)
        {
            return Problem(e.Message);
        }
    }
}