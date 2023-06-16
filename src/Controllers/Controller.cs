using DiscGolfCourseApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DiscGolfCourseApi.Controllers;

public class Controller<T> : ControllerBase where T : Repository
{
    protected T Repo;

    public Controller(T repo)
    {
        Repo = repo;
    }
}