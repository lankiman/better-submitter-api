namespace better_submitter_api;

public static class Services
{
    
    private static IWebHostEnvironment _hostingEnvironment;

    private static ILogger _dataActionsLogger;

    private static ILogger _fileActionsLogger;
    
    private static readonly string _contentRootPath;
    
    private static readonly string _generalDataPath = Path.Combine(_contentRootPath, "Data", "general_submission_data.csv");
    
    private static readonly string _cDataPath = Path.Combine(_contentRootPath, "Data", "c_submission_data.csv");
    
    private static readonly string _pythongDataPath = Path.Combine(_contentRootPath, "Data", "python_submission_data.csv");
    
    private static Dictionary<string, GeneralDataModel> GeneralSubmissionData { get; set; } = new();
    
    private static Dictionary<string, GeneralDataModel> CSubmissionData { get; set; } = new();
    
    private static Dictionary<string, GeneralDataModel> PythonSubmissionData { get; set; } = new();
    
    
    static Services()
    {
        _contentRootPath = _hostingEnvironment.ContentRootPath;
        PopulateDataDicionaries();
        
    }
    
    public static void Initialize(ILoggerFactory loggerFactory)
    {
        _dataActionsLogger = loggerFactory.CreateLogger("DataActions");
        _fileActionsLogger = loggerFactory.CreateLogger("FileActions");
        
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
                    Department = Enum.Parse<StudentDepartment>(lineData[1])
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
    
    private static async Task<bool> UpdateGeneralDataToFile(this GeneralDataModel newData)
    {
        var data = $"{newData.StudentId},{newData.Department}";

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

    public static async Task<IResult> AddOrReturnGeneralStudentSubmissionData(GeneralDataModel generalStudentData)
    {

        if (!GeneralSubmissionData.ContainsKey(generalStudentData.StudentId))
        {
           var status= await generalStudentData.UpdateGeneralDataToFile();
           if (!status)
           {
               return Results.Json(new{Data=generalStudentData, Status=GeneralSubmissionDataStatus.Failed});
           }
           GeneralSubmissionData.Add(generalStudentData.StudentId, generalStudentData);
           _dataActionsLogger.LogInformation($"student data with id {generalStudentData.StudentId} Successfully Added");
              return Results.Json(new{Data=generalStudentData, Status=GeneralSubmissionDataStatus.Added});
        }
        
        Console.WriteLine($"student data with id {generalStudentData.StudentId} already exists");
        return Results.Json(new{Data=generalStudentData, Status=GeneralSubmissionDataStatus.Present});
        
    }
}