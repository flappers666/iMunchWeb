namespace iMunchWeb.Feature.Project.Commands;

public class BaseResult
{
    public bool Success { get; set; }
    public List<string>? Errors { get; set; }
}