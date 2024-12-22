namespace better_submitter_api;

public static class Services
{
    private static readonly string _contentRootPath;
    
    private static IWebHostEnvironment _hostingEnvironment;
    
    private static 
    
    static Services()
    {
        _contentRootPath = _hostingEnvironment.ContentRootPath;
    }
}