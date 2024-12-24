

using System.Text.RegularExpressions;
using Exception = System.Exception;

namespace better_submitter_api;

public static class Services
{
    
    private static ILogger _dataActionsLogger;

    private static ILogger _fileActionsLogger;
    
    private static string _contentRootPath;
    
    private static  string _generalDataPath;

    private static  string _cDataPath; 
    
    private static  string _pythongDataPath;

    private static string _mainDataFolder;
    
    private const long MaxVideoFileSize = 100 * 1024 * 1024; // 100 MB
    private const long MaxCodeFileSize = 1 * 1024 * 1024; // 1 MB

    
    private static Dictionary<string, GeneralDataModel> GeneralSubmissionData { get; set; } = new();
    
    private static Dictionary<string, SubmissionDataModel> CSubmissionData { get; set; } = new();
    
    private static Dictionary<string, SubmissionDataModel> PythonSubmissionData { get; set; } = new();
    
   //initilize service fields like a constructor
    public static void Initialize(ILoggerFactory loggerFactory, string contentRootPath)
    {
        _contentRootPath=contentRootPath;
        _dataActionsLogger = loggerFactory.CreateLogger("DataActions");
        _fileActionsLogger = loggerFactory.CreateLogger("FileActions");
        var mainDataFolder = Path.Combine(_contentRootPath, "Data");
        InitializeDirectories(mainDataFolder);
        PopulateDataDicionaries();
    }

    //private helper methods
    private static void InitializeDirectories(string mainDataFolder)
    {
        Directory.CreateDirectory(mainDataFolder);
        _mainDataFolder = mainDataFolder;
        Directory.CreateDirectory(Path.Combine(_mainDataFolder, "PythonSubmissions"));
        Directory.CreateDirectory(Path.Combine(_mainDataFolder, "CSubmissions"));
        _generalDataPath = Path.Combine(_mainDataFolder, "General_Data.csv");
        _cDataPath = Path.Combine(_mainDataFolder, "C_Data.cvs");
        _pythongDataPath = Path.Combine(_mainDataFolder, "Python_Data.csv");
    }

    private static string GetOrCreateDepartmentDirectory(StudentDepartment department,  SubmissionType submissionType)
    {
        var departmentFolder =
            Directory.CreateDirectory(Path.Combine(_mainDataFolder, submissionType.ToString(), department.ToString()));
        return departmentFolder.FullName;
    }

    private static string GetRandomTempDataFilePath()
    {
        var temp = Path.GetRandomFileName();
        var tempName = $"{Path.GetFileNameWithoutExtension(temp)}.csv";
        return Path.Combine(_mainDataFolder, tempName);
    }
    private static bool IsFileExist (string filePath)
    {
        return File.Exists(filePath);
    }
    
    private static void PopulateDataDicionaries()
    {
        if (IsFileExist(_generalDataPath))
        {
            var data= File.ReadAllLines(_generalDataPath).ToList();
            GeneralSubmissionData = data.ConvertToGeneralDataModel();
        }
        
        if (IsFileExist(_pythongDataPath))
        {
            var data= File.ReadAllLines(_pythongDataPath).ToList();
            PythonSubmissionData = data.ConvertToSubmissionsDataModel();
        }
        
        if (IsFileExist(_cDataPath))
        {
            var data= File.ReadAllLines(_cDataPath).ToList();
            CSubmissionData = data.ConvertToSubmissionsDataModel();
        }
       
    }
    private static void TryDeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (IOException exception)
        {
            Console.WriteLine(exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            Console.WriteLine(exception.Message);
        }
    }
    
    //private helper methods for  data actions
    private static Dictionary<string, GeneralDataModel> ConvertToGeneralDataModel(this List<string> data)
    {
      
        return data
            .Select(line => line.Split(","))
            .ToDictionary(
                lineData => lineData[0],
                lineData => new GeneralDataModel
                {
                    StudentId = lineData[0], 
                    StudentIdType= Enum.Parse<StudentIdType>(lineData[1]),
                    Department = Enum.Parse<StudentDepartment>(lineData[2]),
                    FirstName = lineData[3],
                    SurnName = lineData[5],
                    MiddleName = lineData[5]
                }
            );
    }
    
    private static Dictionary<string,SubmissionDataModel> ConvertToSubmissionsDataModel(this List<string> data)
    {
        return data.Select(line => line.Split(",")).ToDictionary(
            lineData => lineData[0],
            lineData => new SubmissionDataModel
            {
                StudentId = lineData[0],
                Department = Enum.Parse<StudentDepartment>(lineData[1]),
                SubmitedAssigmentCodeFiles = lineData[2].Split("|").ToDictionary(
                    codeFilesData => int.Parse(codeFilesData.Split("^")[0]),
                    codeFilesData => new AssignmentSubmisssion
                    {
                        AssignmentNumber = int.Parse(codeFilesData.Split("^")[0]),
                        SubmissionCount = int.Parse(codeFilesData.Split("^")[1]),
                        CanBeResubmitted = bool.Parse(codeFilesData.Split("^")[2])
                    }),
                SubmitedAssigmentVideoFiles = lineData[3].Split("|").ToDictionary(
                    videoFilesData => int.Parse(videoFilesData.Split("^")[0]),
                    videoFilesData => new AssignmentSubmisssion
                    {
                        AssignmentNumber = int.Parse(videoFilesData.Split("^")[0]),
                        SubmissionCount = int.Parse(videoFilesData.Split("^")[1]),
                        CanBeResubmitted = bool.Parse(videoFilesData.Split("^")[2])
                    }),

            }
        );
    }
    
    private static Dictionary<string,string> ValidateGeneralDataModel(GeneralDataModel model)
    {
        var errors = new Dictionary<string, string>();

        bool IsValidStudentId(string studentId)
        {
            string pattern = @"^UG/\d{2}/\d{4}$";
            Regex regex = new(pattern);
            return regex.IsMatch(studentId.ToUpper());
        }
        void AddErrorIfEmpty(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add(fieldName, $"{fieldName} is required.");
            }
        }

        AddErrorIfEmpty(model.StudentId, nameof(model.StudentId));
        if(!IsValidStudentId(model.StudentId))
        {
            errors.Add(nameof(model.StudentId), "Invalid Student Id");
        }
        AddErrorIfEmpty(model.SurnName, nameof(model.SurnName));
        AddErrorIfEmpty(model.FirstName, nameof(model.FirstName));
        
        
        if (!Enum.IsDefined(typeof(StudentDepartment), model.Department))
        {
            errors.Add(nameof(model.Department), "Invalid Department");
        }

        return errors;
    }
   
    
    private static async Task<bool> AddGeneralDataToFile(this  GeneralDataModel newData)
    {
        var data = $"{newData.StudentId},{newData.StudentIdType},{newData.Department},{newData.SurnName},{newData.FirstName},{newData.MiddleName??" "}";

        try
        {
            using var writer = new StreamWriter(_generalDataPath, append:true);
            await writer.WriteLineAsync(data);
            _dataActionsLogger.LogInformation($"student data with id {newData.StudentId} updated to file");
            return true;
        }
        catch (Exception exception)
        {
         _dataActionsLogger.LogInformation($"An Error Occured while updating student data with id {newData.StudentId} to file");
         _dataActionsLogger.LogError(exception.Message);
         return false;
        }
    }
    
    //endpoint actions for general data
    public static async Task<IResult> CheckForGeneralStudentSubmissionData(string studentId)
    {
        var data = GeneralSubmissionData.TryGetValue(studentId, out var generalData);
        if (!data)
        {
            _dataActionsLogger.LogInformation($"student data with id {studentId} Not Found");
            return Results.Json(new{Status=GeneralSubmissionDataStatus.NotPresent.ToString(), Message="General Student Data Not Found"});
        }
        
        _dataActionsLogger.LogInformation($"student data with id {studentId} Found");
        return Results.Json(new{Data=new GeneralDataModelResponse
        {
            StudentId = generalData.StudentId,
            StudentIdType= generalData.StudentIdType.ToString(),
            Department = generalData.Department.ToString(),
            FirstName = generalData.FirstName,
            SurnName = generalData.SurnName,
            MiddleName = generalData.MiddleName
        }, Status=GeneralSubmissionDataStatus.Present.ToString(), Message="General Student Data Found"});
    }
    
    public static async Task<IResult> AddGeneralStudentSubmissionData(GeneralDataModel generalStudentData)
    {
        var validationErrors = ValidateGeneralDataModel(generalStudentData);
        if (validationErrors.Any())
        {
            return Results.Json(new { Status = GeneralSubmissionDataStatus.Failed.ToString(), Errors = validationErrors, Message="Validation Errors" });
        }
        var data = GeneralSubmissionData.TryGetValue(generalStudentData.StudentId, out var generalData);
        if (data)
        {
            _dataActionsLogger.LogInformation($"student data with id {generalStudentData.StudentId} already Exist");
            
            return Results.Json(new{Data=new GeneralDataModelResponse
            {
                StudentId = generalData.StudentId,
                StudentIdType = generalData.StudentIdType.ToString(),
                Department = generalData.Department.ToString(),
                FirstName = generalData.FirstName,
                SurnName = generalData.SurnName,
                MiddleName = generalData.MiddleName
            }, Status=GeneralSubmissionDataStatus.Present.ToString(), Message="General Student Data Already Exists"});
        }
        var status= await generalStudentData.AddGeneralDataToFile();
           if (!status)
           {
               return Results.Json(new{Status=GeneralSubmissionDataStatus.Failed, Message="An Error Occured while Adding General Student Data"});
           }
           GeneralSubmissionData.Add(generalStudentData.StudentId, generalStudentData);
           _dataActionsLogger.LogInformation($"student data with id {generalStudentData.StudentId} Successfully Added");
           return Results.Json(new{Status=GeneralSubmissionDataStatus.Added.ToString(), Message="General Student Data Successfully Added"});
    }
    
    //TODO: implment method and endpoint to update general student data
    
    //private helper method for file actions

    private static bool ValidateFile(IFormFile file)
    {
        return file == null || file.Length == 0;
    }
    
    private static bool ValidateFileSize(IFormFile file, long fileSize)
    {
        return file.Length <= fileSize;
    }
    
    private static bool ValidateFileTypeAndExtension(IFormFile file, FileType fileType)
    {
       
        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        string contentType = file.ContentType.ToLowerInvariant();
    
        return fileType switch
        {
            FileType.c => extension == ".c" && 
                          (contentType == "text/x-c" || 
                           contentType == "text/plain" ||
                           contentType == "application/octet-stream"),
                      
            FileType.py => extension == ".py" && 
                               (contentType == "text/x-python" || 
                                contentType == "text/plain" ||
                                contentType == "application/octet-stream"),
                           
            FileType.mp4 => extension == ".mp4" && 
                            (contentType == "video/mp4" || 
                             contentType == "application/mp4" ||
                             contentType == "application/octet-stream"),
                         
            _ => false
        };
    }
    
    private static Dictionary<string, string> ValidateFile(IFormFile file, FileType fileType, long fileSize)
    {
        var errors = new Dictionary<string, string>();
        if (ValidateFile(file))
        {
            errors.Add("File", "File is required");
        }
        if (!ValidateFileSize(file, fileSize))
        {
            errors.Add("File", $"File size is too large, maximum acceptable size is {fileSize}");
        }
        if (!ValidateFileTypeAndExtension(file, fileType))
        {
            errors.Add("File", $"Invalid File Type or Extension, must be a {fileType.ToString()} with a .{fileType.ToString()} extension");
        }
        return errors;
    }

    private static StudentIdType GetStudentIdType(string studendId)
    {
      return studendId.StartsWith("UG")? StudentIdType.MatricNumber: StudentIdType.JambRegNumber;
    }
    
    private static string ConvertMatricNumberToNamingFormat(string matricNumber)
    {
        return matricNumber.Replace("/", "_");
    }
    private static string GetFileName(GeneralDataModel data, int assignmentNumber, FileType fileType)
    {
        var studentIdType = GetStudentIdType(data.StudentId);
        var id = data.StudentId;
        if(studentIdType==StudentIdType.MatricNumber) 
        {
          id=ConvertMatricNumberToNamingFormat(data.StudentId);
        }
        return $"{id}_{data.SurnName}_{data.FirstName}{(string.IsNullOrWhiteSpace(data.MiddleName) ? "" : $"_{data.MiddleName}")}_{assignmentNumber}.{fileType}";
    }
    
    private static string GetStudentFolderPath(GeneralDataModel data)
    {
        var studentIdType = GetStudentIdType(data.StudentId);
        var id = data.StudentId;
        if(studentIdType==StudentIdType.MatricNumber)
        {
            id=ConvertMatricNumberToNamingFormat(data.StudentId);
        }
        return $"{id}_{data.SurnName}_{data.FirstName}{(string.IsNullOrWhiteSpace(data.MiddleName) ? "" : $"_{data.MiddleName}")}";
    }
    
    private static (string, string) ComputeCodeFilePath(FileSubmissionRequestModel submissionRequestData)
    {
        var folderPath=GetOrCreateDepartmentDirectory(submissionRequestData.StudentData.Department, submissionRequestData.SubmissionType);
        
        var studentFolderPath=GetStudentFolderPath(submissionRequestData.StudentData);
        
        var fileName = GetFileName(submissionRequestData.StudentData, submissionRequestData.AssignmentNumber, FileType.py);
        
        var filePath = Path.Combine(folderPath, studentFolderPath, fileName);
        return (filePath, fileName);
    }
    
    private static async Task<bool> SaveCodeFileToStorage(this FileSubmissionRequestModel submissionRequestData)
    {

        var (filePath, fileName) = ComputeCodeFilePath(submissionRequestData);

        try
        {
            using var stream = new FileStream(filePath, FileMode.Create);
            await submissionRequestData.File.CopyToAsync(stream);
            _fileActionsLogger.LogInformation($"File {fileName} saved to storage");
            return true;

        }catch (Exception exception)
        {
            _fileActionsLogger.LogInformation($"An error occured while saving file {fileName} to storage");
            _fileActionsLogger.LogError(exception.Message);
            TryDeleteFile(filePath);
            return false;
        }
        
    }
    
    private static async Task<bool> AddCodeSubmissionDataToFile(this SubmissionDataModel newData, string dataFilePath)
    {
        var submissionData = newData.SubmitedAssigmentCodeFiles.FirstOrDefault().Value;
        var data = $"{newData.StudentId},{newData.Department},{submissionData.AssignmentNumber}^{submissionData.SubmissionCount}^{submissionData.CanBeResubmitted},{""}";

        try
        {
            using var writer = new StreamWriter(dataFilePath, append:true);
            await writer.WriteLineAsync(data);
            _dataActionsLogger.LogInformation($"python code file for {newData.StudentId} assigment number {submissionData.AssignmentNumber} added to file");
            return true;
        }
        catch (Exception exception)
        {
            _dataActionsLogger.LogInformation($"error occured while adding code file for {newData.StudentId} assignment number {submissionData.AssignmentNumber}");
            _dataActionsLogger.LogError(exception.Message);
            return false;
        }
    }

    private static Dictionary<int, AssignmentSubmisssion> ConvertToAssignmentSubmissionDataDictionary(this string data)
    {
        
    }

    private static string UpdateAssignmentSubmissionDataString(this string oldData, string dataToUpdate, string newData)
    {
        
    }
    private static async Task<bool> UpdateCodeSubmissionDataToFile(this SubmissionDataModel newData, string filePath)
    {
        var submissionData = newData.SubmitedAssigmentCodeFiles.FirstOrDefault().Value;
        var tempFilePath = GetRandomTempDataFilePath();
        try
        {
            using var reader = new StreamReader(filePath);
            using var writer = new StreamWriter(tempFilePath);

            string? line;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                string[] parts = line.Split(",");

                if (parts[0] == newData.StudentId)
                {
                    string newValue = "";
                    parts[2] = newValue;
                    await writer.WriteLineAsync(string.Join(",", parts));
                }

                await writer.WriteLineAsync(line);
            }
            
            
        }
        catch (Exception exception)
        {
            _dataActionsLogger.LogInformation(
                $"error occured while adding updating code file for {newData.StudentId}  assignment number {submissionData.AssignmentNumber}");
            _dataActionsLogger.LogError(exception.Message);
            return false;
        }
        finally
        {
            TryDeleteFile(tempFilePath);
        }
    }

    //private methods for python file submssions
    
    //public endpoint actions for python file submissions
    
    public static async Task<IResult> SubmitPythonCodeFile(FileSubmissionRequestModel submissionRequestData)
    {
        var validationErrors = ValidateFile(submissionRequestData.File, FileType.py, MaxCodeFileSize);
        if (validationErrors.Any())
        {
            return Results.Json(new { Status = FileSubmissionStatus.Failed.ToString(), Errors = validationErrors, Message="Validation Errors" });
        }
        var result= await submissionRequestData.SaveCodeFileToStorage();

        if (!result)
        {
            return Results.Json(new{Status=FileSubmissionStatus.Failed, Message="An Error Occured while Uploading File"}); 
        }
        var data = PythonSubmissionData.TryGetValue(submissionRequestData.StudentData.StudentId, out var generalData);
        
        if (!data)
        {
            _fileActionsLogger.LogInformation($"submiting python code file for student with id {submissionRequestData.StudentData.StudentId} assignment number {submissionRequestData.AssignmentNumber}");
            var model = new SubmissionDataModel
            {
                StudentId = submissionRequestData.StudentData.StudentId,
                Department = submissionRequestData.StudentData.Department,
                SubmitedAssigmentCodeFiles = new Dictionary<int, AssignmentSubmisssion>
                {
                    {
                        submissionRequestData.AssignmentNumber, new AssignmentSubmisssion
                        {
                            AssignmentNumber = submissionRequestData.AssignmentNumber,
                            SubmissionCount = 1,
                            CanBeResubmitted = true
                        }
                    }
                },
                SubmitedAssigmentVideoFiles = new()
            };
            var status = await model.AddCodeSubmissionDataToFile(_pythongDataPath);
            if (!status)
            {
                var (filePath,_) = ComputeCodeFilePath(submissionRequestData);
                TryDeleteFile(filePath);
                return Results.Json(new{Status=FileSubmissionStatus.Failed, Message="An Error Occured while Uploading File"});
            }
            PythonSubmissionData.Add(submissionRequestData.StudentData.StudentId, model);
            return Results.Json(new{Status=FileSubmissionStatus.Successfull, Message="File Sucessfully Uploaded"}); 
        }

    }
}