namespace better_submitter_api;

public static class Services
{
    
    private static IWebHostEnvironment _hostingEnvironment;
    
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
    }
    
    private static void PopulateDataDicionaries()
    {
        if (File.Exists(_generalDataPath))
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
}