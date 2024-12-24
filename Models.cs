namespace better_submitter_api;

public class GeneralDataModel
{
    public string StudentId { get; set; } 
    
    public StudentIdType StudentIdType { get; set; }
    public StudentDepartment Department { get; set; }
    
    public string SurnName { get; set; }
    
    public string FirstName { get; set; }
    
    public string? MiddleName { get; set; }
}

public class SubmissionDataModel
{
    public string StudentId { get; set; } 
    
    public StudentDepartment Department { get; set; }
    
    public Dictionary<int,AssignmentSubmisssion> SubmittedAssignmentCodeFiles { get; set; }
    
    public Dictionary<int,AssignmentSubmisssion> SubmittedAssignmentVideoFiles { get; set; }
}

public class AssignmentSubmisssion
{
    public int AssignmentNumber { get; set; }
    public int SubmissionCount { get; set; }
    public bool CanBeResubmitted { get; set; }
}

public class GeneralDataModelResponse
{
    public string StudentId { get; set; }
    
    public string StudentIdType { get; set; }
    public string Department { get; set; }
    public string FirstName { get; set; }
    public string SurnName { get; set; }
    public string? MiddleName { get; set; }
}

public class FileSubmissionRequestModel
{
  public GeneralDataModel StudentData { get; set; }
  public int AssignmentNumber { get; set; }
  
  public SubmissionFileType SubmissionFileType { get; set; }
  
  public SubmissionFolderType SubmissionFolderType { get; set; }
  
  public FileType FileType { get; set; }
  
  public IFormFile File { get; set; }  
}
public enum AssigmentType
{
    C,
    Python
}

public enum StudentIdType
{
    MatricNumber,
    JambRegNumber
}

public enum FileType
{
    c,
    py,
    mp4,
    java,
}

public enum SubmissionFileType
{
    Code = 2,
    Video = 3
}


public enum SubmissionFolderType
{
    CSubmissions,
    PythonSubmissions,
    JavaSubmissions
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
    Failed,
    NotPresent,
    Updated
}

public enum FileSubmissionStatus
{
    Successfull,
    Failed
}
