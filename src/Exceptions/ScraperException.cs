namespace DiscGolfCourseApi.Exceptions;

public class ScraperException : Exception
{
    public ScraperException(string msg) : base(msg)
    {
    }
    
    public ScraperException(string msg, Exception inner) : base(msg, inner)
    {
    } 
}