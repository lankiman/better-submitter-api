
using System.Runtime.Serialization;

namespace better_submitter_api;

public class GeneralDataModel
{
    public string StudentId { get; set; } 
    
    public StudentIdType StudentIdType { get; set; }
    public StudentDepartment Department { get; set; }
    
    public string Surname { get; set; }
    
    public string Firstname { get; set; }
    
    public string? Middlename { get; set; }
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
    
    public string Department { get; set; }
    public string Firstname { get; set; }
    public string Surname { get; set; }
    public string? Middlename { get; set; }
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
    Python,
    Java
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
    [EnumMember(Value = "Agricultural")]
    Agricultural,  
    [EnumMember(Value = "Civil")]
    Civil,
    [EnumMember(Value = "Chemical")] 
    Chemical,
    [EnumMember(Value = "Electrical")]
    Electrical,
    [EnumMember(Value = "Marine")]
    Marine,
    [EnumMember(Value = "Mechanical")] 
    Mechanical,
    [EnumMember(Value = "Petroleum")]
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

public class AssignmentMetaDataResponse
{
    public int MaxAssignmentNumber { get; set; }
    
    public List<int> NotSubmittableCodeFiles { get; set; }
    
    public List<int> NotSubmittableVideoFiles { get; set; }
}
