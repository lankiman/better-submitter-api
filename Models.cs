namespace better_submitter_api;

public class GeneralDataModel
{
    public string StudentId { get; set; } 
    public StudentDepartment Department { get; set; }
}

public class SubmissionDataModel:GeneralDataModel
{
    public string SurnName { get; set; }
    
    public string FirstName { get; set; }
    
    public string? MiddleName { get; set; }
    
    public Dictionary<int,AssignmentSubmisssion> SubmitedAssigmentFiles { get; set; }
    
    public Dictionary<int,AssignmentSubmisssion> SubmitedAssigmentVideos { get; set; }
}

public class AssignmentSubmisssion
{
    public int SubmissionCount { get; set; }
    public bool CanBeResubmitted { get; set; }
}
public enum AssigmentType
{
    C,
    Python
}

public enum StudentDepartment
{
    Agricultural,
    Civil,
    Chemical,
    Electrical,
    Mechanical,
    Marine,
    Petroleum
}

public enum GeneralSubmissionDataStatus
{
    Present,
    Added,
    Failed
}
