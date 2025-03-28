
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

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
  public string StudentId { get; set; }
  public int AssignmentNumber { get; set; }
  
  public SubmissionFileType SubmissionFileType { get; set; }
  
  public AssigmentType AssignmentType { get; set; }
  public FileType FileType { get; set; }
  
  public IFormFile File { get; set; }  
}


public enum AssigmentType
{
    [EnumMember(Value = "C")] 
    C,
    [EnumMember(Value = "Python")] 
    Python,
    [EnumMember(Value = "Java")] 
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

// public record AssignmentNumber
// {
//     public int Java {get; set; }
//     
//     public int C { get; set; }
//     
//     public int Python {get; set; }
// }
//
// public record EnableSubmission
// {
//     public bool Video { get; set; }
//     
//     public bool Code { get; set; }
// }
// public class Settings
// {
//     public EnableSubmission EnableSubmission {get; set; }
//     
//     public AssignmentNumber AssignmentNumber { get; set; }
//     
//     public int MaxCodeFileSize { get; set; }
//     
//     public int MaxVideoFileSize { get; set; }
//     
// }


public record AssignmentNumber
{
    [JsonPropertyName("java")]
    public int Java { get; set; }
    
    [JsonPropertyName("c")]
    public int C { get; set; }
    
    [JsonPropertyName("python")]
    public int Python { get; set; }
}

public record EnableSubmission
{
    [JsonPropertyName("video")]
    public bool Video { get; set; } = false;
    
    [JsonPropertyName("code")]
    public bool Code { get; set; } = false;
}

public record LanguageSubmission
{
    [JsonPropertyName("java")]
    public EnableSubmission Java { get; set; } = new EnableSubmission();
    
    [JsonPropertyName("c")]
    public EnableSubmission C { get; set; } = new EnableSubmission();
    
    [JsonPropertyName("python")]
    public EnableSubmission Python { get; set; } = new EnableSubmission();
}

public class Settings
{
    [JsonPropertyName("enableSubmission")]
    public bool EnableSubmission { get; set; }
    
    [JsonPropertyName("enableLanguageSubmission")]
    public LanguageSubmission EnableLanguageSubmission { get; set; } = new LanguageSubmission();
    
    [JsonPropertyName("assignmentNumber")]
    public AssignmentNumber AssignmentNumber { get; set; } = new AssignmentNumber();
    
    [JsonPropertyName("maxCodeFileSize")]
    public int MaxCodeFileSize { get; set; } = 1 * 1024 * 1024; // Default 1MB
    
    [JsonPropertyName("maxVideoFileSize")]
    public int MaxVideoFileSize { get; set; } = 100 * 1024 * 1024; // Default 100MB
    
    public  Dictionary<string, int> MaxAssignmentNumber => new Dictionary<string, int>
    {
        { "Python", AssignmentNumber.Python},
        { "Java", AssignmentNumber.Java  },
        { "C",AssignmentNumber.C }
    };
}
