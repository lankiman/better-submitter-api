namespace better_submitter_api;

public static class Routes
{
    public static void Register(this WebApplication app)
    {
        app.MapGet("/", () => "Hello World!");
        // app.MapGet("/student", (string studentId) => Services.CheckForGeneralStudentSubmissionData(studentId));
        app.MapGet("/student", (HttpRequest request, string studentId) => 
        {
            if (request.Query.Count > 1)  
            {
                return Results.NotFound();
            }
            return Services.CheckForGeneralStudentSubmissionData(studentId);
        });
        
        app.MapPost("/student",(GeneralDataModel generalStudentData)=> Services.AddGeneralStudentSubmissionData(generalStudentData));
        
        app.MapPut("/student",(GeneralDataModel generalStudentData, string studentId)=> Services.UpdateGeneralStudentSubmissionData(generalStudentData, studentId));
    }
}