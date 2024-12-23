namespace better_submitter_api;

public static class Routes
{
    public static void Register(this WebApplication app)
    {
        app.MapGet("/hello", () => "Hello World!");
        
        app.MapGet("/student/{studentId}", Services.CheckForGeneralStudentSubmissionData);
        
        app.MapPost("/student",(GeneralDataModel generalStudentData)=> Services.AddGeneralStudentSubmissionData(generalStudentData));
    }
}