using DiscGolfCourseApi.Models;

namespace DiscGolfCourseApi.Repositories;

public class Repository
{
    protected DiscgolfDbContext Db;

    public Repository(DiscgolfDbContext db)
    {
        Db = db;
    }
}