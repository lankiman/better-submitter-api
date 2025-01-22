namespace better_submitter_api;

public static class Routes
{
    public static void Register(this WebApplication app)
    {
        app.MapGet("/", () => "Hello World!");
        app.MapGet("/student", (HttpRequest request) => 
        {
            var studentId = request.Query["studentId"].ToString();
            if (request.Query.ContainsKey("assignmentType"))
            {
                var assignmentType = request.Query["assignmentType"].ToString();
                if (request.Query.Count > 2)  
                {
                    return Results.NotFound();
                }
                return Services.GetAssignmentMetaData(studentId, assignmentType);
            }

            if (request.Query.Count > 1)
            {
                return Results.NotFound();
                
            } 
            return Services.CheckForGeneralStudentSubmissionData(studentId);
            
        });
        app.MapPost("/student",(GeneralDataModel generalStudentData)=> Services.AddGeneralStudentSubmissionData(generalStudentData));
        
        app.MapPut("/student",(GeneralDataModel generalStudentData, string studentId)=> Services.UpdateGeneralStudentSubmissionData(generalStudentData, studentId));

        app.MapPost("/student/upload",
            (IFormFile file, FileSubmissionRequestModel submissionRequestData) => Services.SubmitAssignmentFile(submissionRequestData));
    }
}