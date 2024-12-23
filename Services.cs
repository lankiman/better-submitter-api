namespace better_submitter_api;

public static class Services
{
    
    private static ILogger _dataActionsLogger;

    private static ILogger _fileActionsLogger;
    
    private static string _contentRootPath;
    
    private static  string _generalDataPath;

    private static  string _cDataPath; 
    
    private static  string _pythongDataPath; 
    
    private static Dictionary<string, GeneralDataModel> GeneralSubmissionData { get; set; } = new();
    
    private static Dictionary<string, SubmissionDataModel> CSubmissionData { get; set; } = new();
    
    private static Dictionary<string, SubmissionDataModel> PythonSubmissionData { get; set; } = new();
    
    
    static Services(){}
    
    public static void Initialize(ILoggerFactory loggerFactory, string contentRootPath)
    {
        _contentRootPath=contentRootPath;
        _dataActionsLogger = loggerFactory.CreateLogger("DataActions");
        _fileActionsLogger = loggerFactory.CreateLogger("FileActions");
        var mainDataFolder = Path.Combine(_contentRootPath, "Data");
        Directory.CreateDirectory(mainDataFolder);
        _generalDataPath = Path.Combine(mainDataFolder, "GeneralData.txt");
        _cDataPath = Path.Combine(mainDataFolder, "CData.txt");
        _pythongDataPath = Path.Combine(mainDataFolder, "PythonData.txt");
        PopulateDataDicionaries();
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
    
    private static Dictionary<string, GeneralDataModel> ConvertToGeneralDataModel(this List<string> data)
    {
      
        return data
            .Select(line => line.Split(","))
            .ToDictionary(
                lineData => lineData[0],
                lineData => new GeneralDataModel
                {
                    StudentId = lineData[0],
                    Department = Enum.Parse<StudentDepartment>(lineData[1]),
                    FirstName = lineData[2],
                    SurnName = lineData[3],
                    MiddleName = lineData[4]
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
    
    private static async Task<bool> AddGeneralDataToFile(this GeneralDataModel newData)
    {
        var data = $"{newData.StudentId},{newData.Department},{newData.FirstName},{newData.SurnName},{newData.MiddleName??""}";

        try
        {
            using var writer = new StreamWriter(_generalDataPath);
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
    
    public static async Task<IResult> CheckForGeneralStudentSubmissionData(string studentId)
    {
        var data = GeneralSubmissionData.TryGetValue(studentId, out var generalData);
        if (!data)
        {
            return Results.Json(new{Status=GeneralSubmissionDataStatus.NotPresent, Message="General Student Data Not Found"});
        }
        
        return Results.Json(new{Data=generalData, Status=GeneralSubmissionDataStatus.Present, Message="General Student Data Found"});
    }

    public static async Task<IResult> AddGeneralStudentSubmissionData(GeneralDataModel generalStudentData)
    {
           var status= await generalStudentData.AddGeneralDataToFile();
           if (!status)
           {
               return Results.Json(new{Status=GeneralSubmissionDataStatus.Failed, Message="An Error Occured while Adding General Student Data"});
           }
           GeneralSubmissionData.Add(generalStudentData.StudentId, generalStudentData);
           _dataActionsLogger.LogInformation($"student data with id {generalStudentData.StudentId} Successfully Added");
           return Results.Json(new{Data=generalStudentData, Status=GeneralSubmissionDataStatus.Added, Message="General Student Data Successfully Added"});
        
    }
}