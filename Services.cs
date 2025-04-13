using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Exception = System.Exception;

namespace better_submitter_api;

public static class Services
{
    private static ILogger _dataActionsLogger;

    private static ILogger _fileActionsLogger;

    private static string _contentRootPath;

    private static string _generalDataPath;

    private static string _submitterSettingsDataPath;

    private static string _cDataPath;

    private static string _pythongDataPath;

    private static string _javaDataPath;

    private static string _mainDataFolder;

    // private const long MaxVideoFileSize = 100 * 1024 * 1024; // 100 MB
    // private const long MaxCodeFileSize = 1 * 1024 * 1024; // 1 MB
    
    private static Dictionary<string, GeneralDataModel> GeneralSubmissionData { get; set; } = new();

    private static Dictionary<string, SubmissionDataModel> CSubmissionData { get; set; } = new();

    private static Dictionary<string, SubmissionDataModel> PythonSubmissionData { get; set; } = new();

    private static Dictionary<string, SubmissionDataModel> JavaSubmissionData { get; set; } = new();

    private static Settings SubmitterSettings { get; set; } = new()
        {
            EnableSubmission = true,
            EnableLanguageSubmission = new LanguageSubmission
            {
                Java = new EnableSubmission
                {
                    Video = true,
                    Code = true
                },
                C = new EnableSubmission
                {
                    Video = true,
                    Code = true
                },
                Python = new EnableSubmission
                {
                    Video = true,
                    Code = true
                }
            },
            AssignmentNumber = new AssignmentNumber
            {
                Java = 15,
                C = 15,
                Python = 15
            },
            MaxCodeFileSize = 1 * 1024 * 1024,
            MaxVideoFileSize = 1000 * 1024 * 1024
        
    };
    
    //initilize service fields like a constructor
    public static void Initialize(ILoggerFactory loggerFactory, string contentRootPath)
    {
        _contentRootPath = contentRootPath;
        _dataActionsLogger = loggerFactory.CreateLogger("DataActions");
        _fileActionsLogger = loggerFactory.CreateLogger("FileActions");
        var mainDataFolder = Path.Combine(_contentRootPath, "Data");
        InitializeDirectories(mainDataFolder);
        PopulateDataDicionaries();
    }

    //private helper methods
    private static void InitializeDirectories(string mainDataFolder)
    {
        Console.WriteLine($"Creating main directory: {mainDataFolder}");
        Directory.CreateDirectory(mainDataFolder);
        _mainDataFolder = mainDataFolder;
        Directory.CreateDirectory(Path.Combine(_mainDataFolder, "PythonSubmissions"));
        Directory.CreateDirectory(Path.Combine(_mainDataFolder, "CSubmissions"));
        Directory.CreateDirectory(Path.Combine(_mainDataFolder, "JavaSubmissions"));
        _generalDataPath = Path.Combine(_mainDataFolder, "General_Data.csv");
        _cDataPath = Path.Combine(_mainDataFolder, "C_Data.csv");
        _pythongDataPath = Path.Combine(_mainDataFolder, "Python_Data.csv");
        _javaDataPath = Path.Combine(_mainDataFolder, "Java_Data.csv");
        _submitterSettingsDataPath = Path.Combine(_mainDataFolder, "submitter_config.json");
    }


    private static GeneralDataModel GetStudentData(string studentId)
    {
        return GeneralSubmissionData[studentId];
    }

    private static string GetOrCreateDepartmentDirectory(SubmissionFolderType submissionFolderType,
        StudentDepartment department)
    {
        var departmentFolder =
            Directory.CreateDirectory(Path.Combine(_mainDataFolder, submissionFolderType.ToString(),
                department.ToString()));
        return departmentFolder.FullName;
    }

    private static SubmissionFolderType ComputeSubmisssionFolderType(AssigmentType assigmentType)
    {
        return assigmentType switch
        {
            AssigmentType.C => SubmissionFolderType.CSubmissions,
            AssigmentType.Java => SubmissionFolderType.JavaSubmissions,
            AssigmentType.Python => SubmissionFolderType.PythonSubmissions,
            _ => throw new ArgumentOutOfRangeException(nameof(assigmentType), assigmentType, null)
        };
    }

    private static string GetRandomTempDataFilePath()
    {
        var temp = Path.GetRandomFileName();
        var tempName = $"{Path.GetFileNameWithoutExtension(temp)}.csv";
        return Path.Combine(_mainDataFolder, tempName);
    }

    private static bool IsFileExist(string filePath)
    {
        return File.Exists(filePath);
    }

    private static void PopulateDataDicionaries()
    {
        if (IsFileExist(_generalDataPath))
        {
            var data = File.ReadAllLines(_generalDataPath).ToList();
            GeneralSubmissionData = data.ConvertToGeneralDataModel();
        }

        if (IsFileExist(_pythongDataPath))
        {
            var data = File.ReadAllLines(_pythongDataPath).ToList();
            PythonSubmissionData = data.ConvertToSubmissionsDataModel();
        }

        if (IsFileExist(_cDataPath))
        {
            var data = File.ReadAllLines(_cDataPath).ToList();
            CSubmissionData = data.ConvertToSubmissionsDataModel();
        }

        if (IsFileExist(_javaDataPath))
        {
            var data = File.ReadAllLines(_javaDataPath).ToList();
            JavaSubmissionData = data.ConvertToSubmissionsDataModel();
        }

        if (IsFileExist(_submitterSettingsDataPath))
        {
            var data = JsonSerializer.Deserialize<Settings>(File
                .ReadAllText(_submitterSettingsDataPath));
            SubmitterSettings = data;
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

    private static bool IsValidStudentId(string studentId)
    {
        string matNoPattern = @"^UG/\d{2}/\d{4}$";
        string regNoPattern = @"^\d{12}[A-Z]{2}$";
        Regex matNoRegex = new(matNoPattern);
        Regex regNorRegex = new(regNoPattern);
        return (matNoRegex.IsMatch(studentId.ToUpper()) || regNorRegex.IsMatch(studentId.ToUpper()));
    }

    //private helper methods for  data actions


    private static Dictionary<string, int> ConvertToMaxAssignmentNumberDictionary(this List<string> data)
    {
        return data.Select(line =>
        {
            var lineData = line.Split(",");
            return new KeyValuePair<string, int>(lineData[0], int.Parse(lineData[1]));
        }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
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
                    StudentIdType = Enum.Parse<StudentIdType>(lineData[1]),
                    Department = Enum.Parse<StudentDepartment>(lineData[2]),
                    Firstname = lineData[3],
                    Surname = lineData[4],
                    Middlename = lineData.Length > 5 ? lineData[5] : ""
                }
            );
    }

    // private static Dictionary<string, SubmissionDataModel> ConvertToSubmissionsDataModel(this List<string> data)
    // {
    //     return data.Select(line => line.Split(",")).ToDictionary(
    //         lineData => lineData[0],
    //         lineData => new SubmissionDataModel
    //         {
    //             StudentId = lineData[0],
    //             Department = Enum.Parse<StudentDepartment>(lineData[1]),
    //             SubmittedAssignmentCodeFiles = lineData[2].Split("|").ToDictionary(
    //                 codeFilesData => int.Parse(codeFilesData.Split("^")[0]),
    //                 codeFilesData => new AssignmentSubmisssion
    //                 {
    //                     AssignmentNumber = int.Parse(codeFilesData.Split("^")[0]),
    //                     SubmissionCount = int.Parse(codeFilesData.Split("^")[1]),
    //                     CanBeResubmitted = bool.Parse(codeFilesData.Split("^")[2])
    //                 }),
    //             SubmittedAssignmentVideoFiles = lineData[3].Split("|").ToDictionary(
    //                 videoFilesData => int.Parse(videoFilesData.Split("^")[0]),
    //                 videoFilesData => new AssignmentSubmisssion
    //                 {
    //                     AssignmentNumber = int.Parse(videoFilesData.Split("^")[0]),
    //                     SubmissionCount = int.Parse(videoFilesData.Split("^")[1]),
    //                     CanBeResubmitted = bool.Parse(videoFilesData.Split("^")[2])
    //                 }),
    //         }
    //     );
    // }
    
    private static Dictionary<string, SubmissionDataModel> ConvertToSubmissionsDataModel(this List<string> data)
    {
        return data.Select(line => line.Split(",")).ToDictionary(
            lineData => lineData[0], // StudentId as the key
            lineData => new SubmissionDataModel
            {
                StudentId = lineData[0],
                Department = Enum.Parse<StudentDepartment>(lineData[1]),
                SubmittedAssignmentCodeFiles = string.IsNullOrEmpty(lineData[2]) 
                    ? new Dictionary<int, AssignmentSubmisssion>() 
                    : lineData[2]
                        .Split("|")
                        .Select(fileData => fileData.Split("^"))
                        .ToDictionary(
                            splitData => int.Parse(splitData[0]), // AssignmentNumber
                            splitData => new AssignmentSubmisssion
                            {
                                AssignmentNumber = int.Parse(splitData[0]),
                                SubmissionCount = int.Parse(splitData[1]),
                                CanBeResubmitted = bool.Parse(splitData[2])
                            }),

                SubmittedAssignmentVideoFiles = string.IsNullOrEmpty(lineData[3]) 
                    ? new Dictionary<int, AssignmentSubmisssion>() 
                    : lineData[3]
                        .Split("|")
                        .Select(fileData => fileData.Split("^"))
                        .ToDictionary(
                            splitData => int.Parse(splitData[0]), // AssignmentNumber
                            splitData => new AssignmentSubmisssion
                            {
                                AssignmentNumber = int.Parse(splitData[0]),
                                SubmissionCount = int.Parse(splitData[1]),
                                CanBeResubmitted = bool.Parse(splitData[2])
                            }),
            }
        );
    }
    
    private static bool IsValidName(string name)
    {
        string namePattern = @"^[A-Za-z]+(-[A-Za-z]+)*$";
        Regex nameRegex = new(namePattern);
        return nameRegex.IsMatch(name);
    }


    private static Dictionary<string, string> ValidateGeneralDataModel(GeneralDataModel model)
    {
        var errors = new Dictionary<string, string>();

        void AddErrorIfEmpty(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add(fieldName, $"{fieldName} is required.");
            }
        }

        AddErrorIfEmpty(model.StudentId, nameof(model.StudentId));
        if (!string.IsNullOrWhiteSpace(model.StudentId) && !IsValidStudentId(model.StudentId))
        {
            errors.Add(nameof(model.StudentId), "Invalid Student Id");
        }

        AddErrorIfEmpty(model.Surname, nameof(model.Surname));
        AddErrorIfEmpty(model.Firstname, nameof(model.Firstname));


        if (!string.IsNullOrWhiteSpace(model.Surname)&&!IsValidName(model.Surname))
        {
            errors.Add(nameof(model.Surname), "Invalid Surname format");
        }
        if (!string.IsNullOrWhiteSpace(model.Firstname)&&!IsValidName(model.Firstname))
        {
            errors.Add(nameof(model.Firstname), "Invalid Firstname format");
        }
        if (!string.IsNullOrWhiteSpace(model.Middlename)&&!IsValidName(model.Middlename))
        {
            errors.Add(nameof(model.Middlename), "Invalid Middename format");
        }

        if (!Enum.TryParse<StudentDepartment>(model.Department.ToString(), true, out var parsedDepartment) ||
            !Enum.IsDefined(typeof(StudentDepartment), parsedDepartment))
        {
            errors.Add(nameof(model.Department), $"'{model.Department}' is not a valid department.");
        }

        return errors;
    }

    //TODO: implmenent custom model bindging and deserializatin for enum types

    private static async Task<bool> AddGeneralDataToFile(this GeneralDataModel newData)
    {
        var data =
            $"{newData.StudentId},{newData.StudentIdType},{newData.Department},{newData.Firstname},{newData.Surname}{(string.IsNullOrWhiteSpace(newData.Middlename) ? "" : $",{newData.Middlename}")}";

        try
        {
            using var writer = new StreamWriter(_generalDataPath, append: true);
            await writer.WriteLineAsync(data);
            _dataActionsLogger.LogInformation($"student data with id {newData.StudentId} updated to file");
            return true;
        }
        catch (Exception exception)
        {
            _dataActionsLogger.LogInformation(
                $"An Error Occured while adding student data with id {newData.StudentId} to file");
            _dataActionsLogger.LogError(exception.Message);
            return false;
        }
    }

    private static async Task<bool> UpdateGeneralStudentDataToFile(this GeneralDataModel newData, string studentId)
    {
        var data =
            $"{newData.StudentId},{newData.StudentIdType},{newData.Department},{newData.Firstname},{newData.Surname}{(string.IsNullOrWhiteSpace(newData.Middlename) ? "" : $",{newData.Middlename}")}";

        var tempFilePath = GetRandomTempDataFilePath();
        try
        {
            using var reader = new StreamReader(_generalDataPath);
            using var writer = new StreamWriter(tempFilePath);

            string? line;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                string[] parts = line.Split(",");


                if (parts[0] == studentId)
                {
                    await writer.WriteLineAsync(data);
                    _dataActionsLogger.LogInformation(
                        $"successfully updated general submission data for {newData.StudentId}");
                }
                else
                {
                    await writer.WriteLineAsync(line);
                }
            }

            File.Move(tempFilePath, _generalDataPath, true);
        }
        catch (Exception exception)
        {
            _dataActionsLogger.LogInformation(
                $"error occured while adding updating general submission data for {newData.StudentId}");
            _dataActionsLogger.LogError(exception.Message);
            return false;
        }
        finally
        {
            TryDeleteFile(tempFilePath);
        }

        return true;
    }
    
    private static GeneralDataModel SanitizeGeneralDataModel(GeneralDataModel model)
    {
        if (model is null) return null;
    
        // Sanitize all text fields
        model.Firstname = SanitizeName(model.Firstname);
        model.Middlename = SanitizeName(model.Middlename);
        model.Surname = SanitizeName(model.Surname);
        model.StudentId = SanitizeStudentId(model.StudentId);
        // Sanitize any other text fields
    
        return model;
    }

    private static string SanitizeText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
    
        // Remove problematic characters and normalize formatting
        return text.Replace("'", "")
            .Replace("*", "")
            .Replace(" _", "_")
            .Replace("_ ", "_")
            .Trim();
    }
    
    private static string SanitizeName(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
    
        // Keep only letters and hyphens
        string cleaned = System.Text.RegularExpressions.Regex.Replace(name, @"[^A-Za-z\-]", "");
    
        // Capitalize first letter, make rest lowercase
        if (cleaned.Length > 0)
        {
            cleaned = char.ToUpper(cleaned[0]) + cleaned.Substring(1).ToLower();
        }
    
        return cleaned;
    }
    // private static string SanitizeName(string name)
    // {
    //     if (string.IsNullOrEmpty(name)) return name;
    //
    //     // Remove problematic characters and normalize formatting
    //     string cleaned = name.Replace("'", "")
    //         .Replace("*", "")
    //         .Replace(" _", "_")
    //         .Replace("_ ", "_")
    //         .Trim();
    //
    //     // Capitalize first letter, make rest lowercase
    //     if (cleaned.Length > 0)
    //     {
    //         cleaned = char.ToUpper(cleaned[0]) + cleaned.Substring(1).ToLower();
    //     }
    //
    //     return cleaned;
    // }

    private  static string SanitizeStudentId(string studentId)
    {
        if (string.IsNullOrEmpty(studentId)) return studentId;
    
        // Normalize student ID format (e.g., UG_22_3788)
        var cleaned = SanitizeText(studentId);
    
        // Ensure proper formatting for student IDs
        // This depends on your expected format
        // Example: Match pattern like UG_22_3788
        return cleaned;
    }

    //endpoint actions for general data
    public static IResult CheckForGeneralStudentSubmissionData(string studentId)
    {
        var data = GeneralSubmissionData.TryGetValue(studentId, out var generalData);
        if (!data)
        {
            _dataActionsLogger.LogInformation($"student data with id {studentId} Not Found");
            return Results.Json(new
            {
                Status = GeneralSubmissionDataStatus.NotPresent.ToString(), Message = "General Student Data Not Found"
            });
        }

        _dataActionsLogger.LogInformation($"student data with id {studentId} Found");
        return Results.Json(new
        {
            Data = new GeneralDataModelResponse
            {
                StudentId = generalData.StudentId,
                Department = generalData.Department.ToString(),
                Firstname = generalData.Firstname,
                Surname = generalData.Surname,
                Middlename = generalData.Middlename
            },
            Status = GeneralSubmissionDataStatus.Present.ToString(), Message = "General Student Data Found"
        });
    }

   

    public static async Task<IResult> AddGeneralStudentSubmissionData(GeneralDataModel generalStudentData)
    {
        if (generalStudentData == null)
        {
            return Results.UnprocessableEntity("General Student Data is required");
        }

      
        
        
        var validationErrors = ValidateGeneralDataModel(generalStudentData);
        
     
        if (validationErrors.Any())
        {
            return Results.BadRequest(new
            {
                Status = GeneralSubmissionDataStatus.Failed.ToString(), Errors = validationErrors,
                Message = "Validation Errors"
            });
        }

        var data = GeneralSubmissionData.TryGetValue(generalStudentData.StudentId, out var generalData);
        if (data)
        {
            _dataActionsLogger.LogInformation($"student data with id {generalStudentData.StudentId} already Exist");

            return Results.Json(new
            {
                Data = new GeneralDataModelResponse
                {
                    StudentId = generalData.StudentId,
                    Department = generalData.Department.ToString(),
                    Firstname = generalData.Firstname,
                    Surname = generalData.Surname,
                    Middlename = generalData.Middlename
                },
                Status = GeneralSubmissionDataStatus.Present.ToString(), Message = "General Student Data Already Exists"
            });
        }
        // Sanitize the data before validation and processing
        generalStudentData = SanitizeGeneralDataModel(generalStudentData);

        var status = await generalStudentData.AddGeneralDataToFile();
        if (!status)
        {
            return Results.Json(new
            {
                Status = GeneralSubmissionDataStatus.Failed,
                Message = "An Error Occured while Adding General Student Data"
            });
        }

        GeneralSubmissionData.Add(generalStudentData.StudentId, generalStudentData);
        _dataActionsLogger.LogInformation($"student data with id {generalStudentData.StudentId} Successfully Added");
        return Results.Json(new
        {
            Data = new GeneralDataModelResponse
            {
                StudentId = generalStudentData.StudentId,
                Department = generalStudentData.Department.ToString(),
                Firstname = generalStudentData.Firstname,
                Surname = generalStudentData.Surname,
                Middlename = generalStudentData.Middlename
            },
            Status = GeneralSubmissionDataStatus.Added.ToString(), Message = "Your data was successfully added"
        });
    }


    public static async Task<IResult> UpdateGeneralStudentSubmissionData(GeneralDataModel newGeneralStudentData,
        string studentId)
    {
        Console.WriteLine("this method was called");

        var data = GeneralSubmissionData.TryGetValue(studentId, out var generalData);

        if (!data)
        {
            _dataActionsLogger.LogInformation($"student data with id {studentId} Not Found");
            return Results.Json(new
            {
                Status = GeneralSubmissionDataStatus.NotPresent.ToString(),
                Message = "General Student Data Not Found or present for update"
            });
        } 
        var validationErrors = ValidateGeneralDataModel(newGeneralStudentData);
        if (validationErrors.Any())
        {
            return Results.Json(new
            {
                Status = GeneralSubmissionDataStatus.Failed.ToString(), Errors = validationErrors,
                Message = "Unable to update data due to validation Errors"
            });
        }

        var status = await newGeneralStudentData.UpdateGeneralStudentDataToFile(studentId);
        if (!status)
        {
            return Results.Json(new
            {
                Status = GeneralSubmissionDataStatus.Failed.ToString(),
                Message = "An Error Occured while Updating General Student Data"
            });
        }


        GeneralSubmissionData.Remove(studentId);
        GeneralSubmissionData[newGeneralStudentData.StudentId] = newGeneralStudentData;

        //TODO: Implement Updating of student Data Across all Files

        _dataActionsLogger.LogInformation(
            $"student data with id {newGeneralStudentData.StudentId} Successfully Updated");
        return Results.Json(new
        {
            Status = GeneralSubmissionDataStatus.Updated.ToString(),
            Message = "General Student Data Successfully Updated"
        });
    }

    public static IResult GetAssignmentMetaData(string studentId, string assignmentType)
    {
        Console.WriteLine($"gotten from meta data {assignmentType}");
        if (!Enum.TryParse<AssigmentType>(assignmentType, true, out var parsedAssignmentType) ||
            !Enum.IsDefined(typeof(AssigmentType), parsedAssignmentType))
        {
            return Results.BadRequest(new
                { Status = GeneralSubmissionDataStatus.Failed.ToString(), Message = "Invalid Assignment Type" });
        }

        var maxNumber = SubmitterSettings.MaxAssignmentNumber[assignmentType];

        var data = GetSubmissionDataDictionaryByType(parsedAssignmentType);

        if (!data.TryGetValue(studentId, out var generalData))
        {
            _dataActionsLogger.LogInformation(
                $"student data with id {studentId} has not submitted any assignment on {parsedAssignmentType}");
            return Results.Json(new
            {
                Data = new AssignmentMetaDataResponse()
                {
                    MaxAssignmentNumber = maxNumber,
                    NotSubmittableVideoFiles = [],
                    NotSubmittableCodeFiles = []
                },
                Status = GeneralSubmissionDataStatus.NotPresent.ToString(),
                Message = $"Student has not submitted any assignment on {parsedAssignmentType}"
            });
        }

        var codeFilesData = generalData.SubmittedAssignmentCodeFiles;

        var videoFilesData = generalData.SubmittedAssignmentVideoFiles;

        var notSubmittableVideoFiles = videoFilesData.Where(file => file.Value.CanBeResubmitted == false)
            .Select(keys => keys.Key).ToList();

        var notSubmittableCodeFiles = codeFilesData.Where(file => file.Value.CanBeResubmitted == false)
            .Select(keys => keys.Key).ToList();

        return Results.Json(new
        {
            Status = GeneralSubmissionDataStatus.Present.ToString(), Data = new AssignmentMetaDataResponse()
            {
                MaxAssignmentNumber = maxNumber,
                NotSubmittableCodeFiles = notSubmittableCodeFiles,
                NotSubmittableVideoFiles = notSubmittableVideoFiles
            }
        });
    }

    //private helper method for file actions

    private static bool ValidateFile(IFormFile file)
    {
        return file == null || file.Length == 0;
    }

    private static bool ValidateFileSize(IFormFile file, long fileSize)
    {
        return file.Length <= fileSize || file.Length < 1;
    }

    private static bool ValidateFileTypeAndExtension(IFormFile file, FileType fileType)
    {
        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        string contentType = file.ContentType.ToLowerInvariant();

        return fileType switch
        {
            FileType.c => extension == ".c" &&
                          (contentType == "text/x-csrc" ||
                           contentType == "text/x-c" ||
                           contentType == "text/plain" ||
                           contentType == "application/octet-stream"),

            FileType.py => extension == ".py" &&
                           (contentType == "text/x-python" ||
                            contentType == "text/plain" ||
                            contentType == "application/octet-stream"),
            FileType.java => extension == ".java" &&
                             (contentType == "text/x-java" ||
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
            errors.Add("File",
                $"Invalid File Type or Extension, must be a {fileType.ToString()} with a .{fileType.ToString()} extension");
        }

        return errors;
    }

    private static StudentIdType GetStudentIdType(string studendId)
    {
        return studendId.StartsWith("UG") ? StudentIdType.MatricNumber : StudentIdType.JambRegNumber;
    }

    private static string ConvertMatricNumberToNamingFormat(string matricNumber)
    {
        return matricNumber.Replace("/", "_");
    }

    private static string GetFileName(GeneralDataModel data, int assignmentNumber, FileType fileType)
    {
        var studentIdType = GetStudentIdType(data.StudentId);
        var id = data.StudentId;
        if (studentIdType == StudentIdType.MatricNumber)
        {
            id = ConvertMatricNumberToNamingFormat(data.StudentId);
        }

        return
            $"{data.Firstname}_{data.Surname}{(string.IsNullOrWhiteSpace(data.Middlename) ? "" : $"_{data.Middlename}")}_{id}_{assignmentNumber}.{fileType}";
    }

    private static string GetStudentFolderPath(GeneralDataModel data)
    {
        var studentIdType = GetStudentIdType(data.StudentId);
        var id = data.StudentId;
        if (studentIdType == StudentIdType.MatricNumber)
        {
            id = ConvertMatricNumberToNamingFormat(data.StudentId);
        }
        
        string result = $"{data.Firstname}_{data.Surname}";
        if (!string.IsNullOrWhiteSpace(data.Middlename))
            result += $"_{data.Middlename}";
        result += $"_{id}";
        
        return result;
    }

    private static AssigmentType GetParsedAssignmentType(string assignmentType)
    {
        Enum.TryParse<AssigmentType>(assignmentType, true, out var parsedAssignmentType);
        return parsedAssignmentType;
    }


    private static bool ValidateAssignmentType(AssigmentType assigmentType)
    {
        if (!Enum.IsDefined(typeof(AssigmentType), assigmentType))
        {
            return true;
            // return Results.BadRequest(new
            //     { Status = GeneralSubmissionDataStatus.Failed.ToString(), Message = "Invalid Assignment Type" });
        }

        return false;
    }

    private static (string, string) ComputeSubmissionFilePath(FileSubmissionRequestModel submissionRequestData)
    {
        var studentData = GetStudentData(submissionRequestData.StudentId);
        var submissionType = ComputeSubmisssionFolderType(submissionRequestData.AssignmentType);
        var folderPath = GetOrCreateDepartmentDirectory(submissionType,
            studentData.Department);

        var studentFolderPath = GetStudentFolderPath(studentData);

        var fileName = GetFileName(studentData, submissionRequestData.AssignmentNumber,
            submissionRequestData.FileType);
        
        // var submissionFilePath = submissionRequestData.SubmissionFileType ==
        //     SubmissionFileType.Code ? "codeFiles" : "videoFiles";

        var folder = Directory.CreateDirectory(Path.Combine(folderPath, studentFolderPath));

        var filePath = Path.Combine(folder.FullName, fileName);
        return (filePath, fileName);
    }

    private static async Task<bool> SaveFileToStorage(this FileSubmissionRequestModel submissionRequestData)
    {
        var (filePath, fileName) = ComputeSubmissionFilePath(submissionRequestData);

        try
        {
            await using var stream = new FileStream(filePath, FileMode.Create);
            await submissionRequestData.File.CopyToAsync(stream);
            _fileActionsLogger.LogInformation($"File {fileName} saved to storage");
            return true;
        }
        catch (Exception exception)
        {
            _fileActionsLogger.LogInformation($"An error occured while saving file {fileName} to storage");
            _fileActionsLogger.LogError(exception.Message);
            TryDeleteFile(filePath);
            return false;
        }
    }

    private static AssignmentSubmisssion ExtractAssignmentSubmissionData(this SubmissionDataModel newData,
        SubmissionFileType submissionFileType)
    {
        return submissionFileType switch
        {
            SubmissionFileType.Code => newData.SubmittedAssignmentCodeFiles.FirstOrDefault().Value,
            SubmissionFileType.Video => newData.SubmittedAssignmentVideoFiles.FirstOrDefault().Value,
            _ => throw new ArgumentOutOfRangeException(nameof(submissionFileType), "Invalid submission type")
        };
    }

    private static async Task<bool> AddAssignmentSubmissionDataToFile(this SubmissionDataModel newData,
        string dataFilePath, SubmissionFileType submissionFileType)
    {
        var submissionData = newData.ExtractAssignmentSubmissionData(submissionFileType);
        var submissionDataPart = submissionFileType switch
        {
            SubmissionFileType.Code =>
                $"{submissionData.AssignmentNumber}^{submissionData.SubmissionCount}^{submissionData.CanBeResubmitted}",
            SubmissionFileType.Video =>
                $"{submissionData.AssignmentNumber}^{submissionData.SubmissionCount}^{submissionData.CanBeResubmitted}",
            _ => throw new ArgumentOutOfRangeException(nameof(submissionFileType), "Invalid submission type")
        };

        var data = submissionFileType switch
        {
            SubmissionFileType.Code => $"{newData.StudentId},{newData.Department},{submissionDataPart},{""}",
            SubmissionFileType.Video => $"{newData.StudentId},{newData.Department},{""},{submissionDataPart}",
            _ => throw new ArgumentOutOfRangeException(nameof(submissionFileType), "Invalid submission type")
        };

        try
        {
            await using var writer = new StreamWriter(dataFilePath, append: true);
            await writer.WriteLineAsync(data);
            _dataActionsLogger.LogInformation(
                $"add {submissionFileType} file metadata for {newData.StudentId} assigment number {submissionData.AssignmentNumber} added to file");
            return true;
        }
        catch (Exception exception)
        {
            _dataActionsLogger.LogInformation(
                $"error occured while adding {submissionFileType} file metadata for {newData.StudentId} assignment number {submissionData.AssignmentNumber}");
            _dataActionsLogger.LogError(exception.Message);
            return false;
        }
    }

    private static Dictionary<int, AssignmentSubmisssion> ConvertToAssignmentSubmissionDataDictionary(this string data)
    
    {
        if (!String.IsNullOrEmpty(data))
        {
            return data.Split("|").ToDictionary(submissionData => int.Parse(submissionData.Split("^")[0]),
                submissionData => new AssignmentSubmisssion
                {
                    AssignmentNumber = int.Parse(submissionData.Split("^")[0]),
                    SubmissionCount = int.Parse(submissionData.Split("^")[1]),
                    CanBeResubmitted = bool.Parse(submissionData.Split("^")[2])
                });
        }

        return new Dictionary<int, AssignmentSubmisssion>();
    }

    private static string ConvertToAssignmentSubmissionDataString(this Dictionary<int, AssignmentSubmisssion> data)
    {
        Console.WriteLine($"now here and here too {data}");
        return string.Join("|",
            data.Select(submissionData =>
                $"{submissionData.Value.AssignmentNumber}^{submissionData.Value.SubmissionCount}^{submissionData.Value.CanBeResubmitted}"));
    }

    private static string UpdateAssignmentSubmissionDataString(this string oldData, int dataToUpdate,
        AssignmentSubmisssion newData)
    {
        var oldDataDictionary = ConvertToAssignmentSubmissionDataDictionary(oldData);
        
        oldDataDictionary[dataToUpdate] = newData;

        return oldDataDictionary.ConvertToAssignmentSubmissionDataString();
    }

    private static async Task<bool> UpdateAssignmentSubmissionDataToFile(this SubmissionDataModel newData,
        string filePath, SubmissionFileType type)
    {
        var submissionData = newData.ExtractAssignmentSubmissionData(type);
        var tempFilePath = GetRandomTempDataFilePath();
        try
        {
            using var reader = new StreamReader(filePath);
            using var writer = new StreamWriter(tempFilePath);

            string? line;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                string[] parts = line.Split(",");

                var index = (int)type;

                if (parts[0] == newData.StudentId)
                {
                    string newValue = parts[index]
                        .UpdateAssignmentSubmissionDataString(submissionData.AssignmentNumber, submissionData);
                    
                    parts[index] = newValue;
                    await writer.WriteLineAsync(string.Join(",", parts));
                    _dataActionsLogger.LogInformation(
                        $"successfully updated {type} file submission data for {newData.StudentId}  assignment number {submissionData.AssignmentNumber}");
                }
                else
                {
                    await writer.WriteLineAsync(line);
                }
            }

            File.Move(tempFilePath, filePath, true);
        }
        catch (Exception exception)
        {
            _dataActionsLogger.LogInformation(
                $"error occured while updating {type} file submission data for {newData.StudentId}  assignment number {submissionData.AssignmentNumber}");
            _dataActionsLogger.LogError(exception.Message);
            return false;
        }
        finally
        {
            TryDeleteFile(tempFilePath);
        }

        return true;
    }

    private static SubmissionDataModel GenerateAddAssignmentSubmissionRequestDataModel(
        this FileSubmissionRequestModel submissionRequestData)
    {
        var studentData = GetStudentData(submissionRequestData.StudentId);
        if (submissionRequestData.SubmissionFileType == SubmissionFileType.Code)
        {
            return new SubmissionDataModel
            {
                StudentId = studentData.StudentId,
                Department = studentData.Department,
                SubmittedAssignmentCodeFiles = new Dictionary<int, AssignmentSubmisssion>
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
                SubmittedAssignmentVideoFiles = new()
            };
        }

        return new SubmissionDataModel
        {
            StudentId = studentData.StudentId,
            Department = studentData.Department,
            SubmittedAssignmentCodeFiles = new(),
            SubmittedAssignmentVideoFiles = new Dictionary<int, AssignmentSubmisssion>
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
        };
    }

    // private static SubmissionDataModel GenerateUpdateAssignmentSubmissionRequestDataModel(
    //     this SubmissionDataModel currentGeneralData, int assignmentNumber, SubmissionFileType type)
    // {
    //     if (type == SubmissionFileType.Code)
    //     {
    //         var currentCodeSubmissionCount = currentGeneralData
    //             .SubmittedAssignmentCodeFiles[assignmentNumber].SubmissionCount;
    //         return new SubmissionDataModel
    //         {
    //             StudentId = currentGeneralData.StudentId,
    //             Department = currentGeneralData.Department,
    //             SubmittedAssignmentCodeFiles = new Dictionary<int, AssignmentSubmisssion>
    //             {
    //                 {
    //                     assignmentNumber, new AssignmentSubmisssion
    //                     {
    //                         AssignmentNumber = assignmentNumber,
    //                         SubmissionCount = currentCodeSubmissionCount + 1,
    //                         CanBeResubmitted = (currentCodeSubmissionCount +1) < 2
    //                     }
    //                 }
    //             }
    //         };
    //     }
    //
    //     var currentVideoSubmissionCount = currentGeneralData
    //         .SubmittedAssignmentVideoFiles[assignmentNumber].SubmissionCount;
    //     return new SubmissionDataModel
    //     {
    //         StudentId = currentGeneralData.StudentId,
    //         Department = currentGeneralData.Department,
    //         SubmittedAssignmentCodeFiles = new Dictionary<int, AssignmentSubmisssion>
    //         {
    //             {
    //                 assignmentNumber, new AssignmentSubmisssion
    //                 {
    //                     AssignmentNumber = assignmentNumber,
    //                     SubmissionCount = currentVideoSubmissionCount + 1,
    //                     CanBeResubmitted = currentVideoSubmissionCount < 2
    //                 }
    //             }
    //         }
    //     };
    // }

    private static SubmissionDataModel GenerateUpdateAssignmentSubmissionRequestDataModel(
    this SubmissionDataModel currentGeneralData, int assignmentNumber, SubmissionFileType type)
{
    if (type == SubmissionFileType.Code)
    {
        // Check if this assignment number already exists
        int currentCodeSubmissionCount = 0;
        if (currentGeneralData.SubmittedAssignmentCodeFiles.ContainsKey(assignmentNumber))
        {
            currentCodeSubmissionCount = currentGeneralData
                .SubmittedAssignmentCodeFiles[assignmentNumber].SubmissionCount;
        }
        
        return new SubmissionDataModel
        {
            StudentId = currentGeneralData.StudentId,
            Department = currentGeneralData.Department,
            SubmittedAssignmentCodeFiles = new Dictionary<int, AssignmentSubmisssion>
            {
                {
                    assignmentNumber, new AssignmentSubmisssion
                    {
                        AssignmentNumber = assignmentNumber,
                        SubmissionCount = currentCodeSubmissionCount + 1,
                        CanBeResubmitted = (currentCodeSubmissionCount + 1) < 2
                    }
                }
            }
        };
    }
    else // Video submission
    {
        // Check if this assignment number already exists
        int currentVideoSubmissionCount = 0;
        if (currentGeneralData.SubmittedAssignmentVideoFiles.ContainsKey(assignmentNumber))
        {
            currentVideoSubmissionCount = currentGeneralData
                .SubmittedAssignmentVideoFiles[assignmentNumber].SubmissionCount;
        }
        
        return new SubmissionDataModel
        {
            StudentId = currentGeneralData.StudentId,
            Department = currentGeneralData.Department,
            SubmittedAssignmentVideoFiles = new Dictionary<int, AssignmentSubmisssion> // Fixed: this was incorrectly set to CodeFiles
            {
                {
                    assignmentNumber, new AssignmentSubmisssion
                    {
                        AssignmentNumber = assignmentNumber,
                        SubmissionCount = currentVideoSubmissionCount + 1,
                        CanBeResubmitted = (currentVideoSubmissionCount + 1) < 2
                    }
                }
            }
        };
    }
}

    private static void UpdateAssignmentSubmissionDataToDictionary(
    this FileSubmissionRequestModel submissionRequestModel, Dictionary<string, SubmissionDataModel> dictionary)
{
    var studentData = GetStudentData(submissionRequestModel.StudentId);
    
    if (submissionRequestModel.SubmissionFileType == SubmissionFileType.Code)
    {
        // Check if this is the first submission for this assignment number
        if (!dictionary[studentData.StudentId].SubmittedAssignmentCodeFiles.ContainsKey(submissionRequestModel.AssignmentNumber))
        {
            // First submission for this assignment
            dictionary[studentData.StudentId].SubmittedAssignmentCodeFiles[submissionRequestModel.AssignmentNumber] = 
                new AssignmentSubmisssion
                {
                    AssignmentNumber = submissionRequestModel.AssignmentNumber,
                    SubmissionCount = 1,
                    CanBeResubmitted = true // First submission can be resubmitted (count < 2)
                };
        }
        else
        {
            // Update existing submission
            var currentSubmissionCount = dictionary[studentData.StudentId]
                .SubmittedAssignmentCodeFiles[submissionRequestModel.AssignmentNumber].SubmissionCount;
                
            dictionary[studentData.StudentId]
                .SubmittedAssignmentCodeFiles[submissionRequestModel.AssignmentNumber].SubmissionCount =
                currentSubmissionCount + 1;
                
            currentSubmissionCount = dictionary[studentData.StudentId]
                .SubmittedAssignmentCodeFiles[submissionRequestModel.AssignmentNumber].SubmissionCount;
                
            dictionary[studentData.StudentId]
                .SubmittedAssignmentCodeFiles[submissionRequestModel.AssignmentNumber].CanBeResubmitted =
                currentSubmissionCount < 2;
        }
    }
    else // Video submission
    {
        // Check if this is the first submission for this assignment number
        if (!dictionary[studentData.StudentId].SubmittedAssignmentVideoFiles.ContainsKey(submissionRequestModel.AssignmentNumber))
        {
            // First submission for this assignment
            dictionary[studentData.StudentId].SubmittedAssignmentVideoFiles[submissionRequestModel.AssignmentNumber] = 
                new AssignmentSubmisssion
                {
                    AssignmentNumber = submissionRequestModel.AssignmentNumber,
                    SubmissionCount = 1,
                    CanBeResubmitted = true // First submission can be resubmitted (count < 2)
                };
        }
        else
        {
            // Update existing submission
            var currentSubmissionCount = dictionary[studentData.StudentId]
                .SubmittedAssignmentVideoFiles[submissionRequestModel.AssignmentNumber].SubmissionCount;
                
            dictionary[studentData.StudentId]
                .SubmittedAssignmentVideoFiles[submissionRequestModel.AssignmentNumber].SubmissionCount =
                currentSubmissionCount + 1;
                
            currentSubmissionCount = dictionary[studentData.StudentId]
                .SubmittedAssignmentVideoFiles[submissionRequestModel.AssignmentNumber].SubmissionCount;
                
            dictionary[studentData.StudentId]
                .SubmittedAssignmentVideoFiles[submissionRequestModel.AssignmentNumber].CanBeResubmitted =
                currentSubmissionCount < 2;
        }
    }
}
    // private static void UpdateAssignmentSubmissionDataToDictionary(
    //     this FileSubmissionRequestModel submissionRequestModel, Dictionary<string, SubmissionDataModel> dictionary)
    // {
    //     var studentData = GetStudentData(submissionRequestModel.StudentId);
    //     if (submissionRequestModel.SubmissionFileType == SubmissionFileType.Code)
    //     {
    //         var currentSubmissionCount = dictionary[studentData.StudentId]
    //             .SubmittedAssignmentCodeFiles[submissionRequestModel.AssignmentNumber].SubmissionCount;
    //         dictionary[studentData.StudentId]
    //                 .SubmittedAssignmentCodeFiles[submissionRequestModel.AssignmentNumber].SubmissionCount =
    //             currentSubmissionCount + 1;
    //         currentSubmissionCount = dictionary[studentData.StudentId]
    //             .SubmittedAssignmentCodeFiles[submissionRequestModel.AssignmentNumber].SubmissionCount;
    //         dictionary[studentData.StudentId]
    //                 .SubmittedAssignmentCodeFiles[submissionRequestModel.AssignmentNumber].CanBeResubmitted =
    //             currentSubmissionCount < 2;
    //     }
    //     else
    //     {
    //         var currentSubmissionCount = dictionary[studentData.StudentId]
    //             .SubmittedAssignmentVideoFiles[submissionRequestModel.AssignmentNumber].SubmissionCount;
    //         dictionary[studentData.StudentId]
    //                 .SubmittedAssignmentVideoFiles[submissionRequestModel.AssignmentNumber].SubmissionCount =
    //             currentSubmissionCount + 1;
    //         currentSubmissionCount = dictionary[studentData.StudentId]
    //             .SubmittedAssignmentVideoFiles[submissionRequestModel.AssignmentNumber].SubmissionCount;
    //         dictionary[studentData.StudentId]
    //                 .SubmittedAssignmentVideoFiles[submissionRequestModel.AssignmentNumber].CanBeResubmitted =
    //             currentSubmissionCount < 2;
    //     }
    // }


    private static Dictionary<string, Dictionary<string, string>> ValidateSubmissionRequestData(
        FileSubmissionRequestModel submissionRequestData)
    {
        var errors = new Dictionary<string, Dictionary<string, string>>();
        
        if (!GeneralSubmissionData.ContainsKey(submissionRequestData.StudentId))
        {
            errors.Add("StudentData",
                new Dictionary<string, string> { { "InvalidStudent", "Student Data does not exist" } });
            return errors;
        }
        
        Console.WriteLine($"let me see the submision type {submissionRequestData.SubmissionFileType}");

        var maxFileSize = submissionRequestData.SubmissionFileType == SubmissionFileType.Code
            ? SubmitterSettings.MaxCodeFileSize
            : SubmitterSettings.MaxVideoFileSize;

        var fileError = ValidateFile(submissionRequestData.File, submissionRequestData.FileType, maxFileSize);

        var submissionDataDictionary = GetSubmissionDataDictionaryByType(submissionRequestData.AssignmentType);

        // var assigmentMetaDataDictionary = submissionRequestData.SubmissionFileType == SubmissionFileType.Code
        //     ? submissionDataDictionary.TryGetValue(submissionRequestData.StudentId, out var codeFilesSubmission)
        //     : submissionDataDictionary[submissionRequestData.StudentId].SubmittedAssignmentVideoFiles;


        var assigmentMetaDataDictionary =
            submissionDataDictionary.TryGetValue(submissionRequestData.StudentId, out var currentSubmissionMetadata);

        var currentFileSubmissionData = new Dictionary<int, AssignmentSubmisssion>();

        
        if (assigmentMetaDataDictionary) 
        {
            if (submissionRequestData.SubmissionFileType == SubmissionFileType.Code)
            {
                currentFileSubmissionData = currentSubmissionMetadata.SubmittedAssignmentCodeFiles;
            }
            else
            {
                currentFileSubmissionData = currentSubmissionMetadata.SubmittedAssignmentVideoFiles;
            }
        }
        
        Console.WriteLine($"gotten here {submissionRequestData.AssignmentType}");

        var maxAssignmentNumber = SubmitterSettings.MaxAssignmentNumber[submissionRequestData.AssignmentType.ToString()];
        
        if (fileError.Any())
        {
            errors.Add("File", fileError);
        }

        if (!Enum.IsDefined(typeof(FileType), submissionRequestData.FileType))
        {
            errors.Add(nameof(submissionRequestData.FileType),
                new Dictionary<string, string> { { "FileType", "Invalid File Type" } });
        }

        if (!Enum.IsDefined(typeof(SubmissionFileType), submissionRequestData.SubmissionFileType))
        {
            errors.Add(nameof(submissionRequestData.SubmissionFileType),
                new Dictionary<string, string> { { "SubmissionFileType", "Invalid Submission File Type" } });
        }

        if (!Enum.IsDefined(typeof(AssigmentType), submissionRequestData.AssignmentType))
        {
            errors.Add(nameof(submissionRequestData.AssignmentType),
                new Dictionary<string, string> { { "AssignmentType", "Invalid Assignment Type" } });
        }

        if (submissionRequestData.AssignmentNumber > maxAssignmentNumber)
        {
            errors.Add(nameof(submissionRequestData.AssignmentNumber), new Dictionary<string, string> { { "AssignmentNumber", "Assignment number exceeds max submittable number" } });
        }

        // if (assigmentMetaDataDictionary && !currentFileSubmissionData[submissionRequestData.AssignmentNumber].CanBeResubmitted)
        // {
        //     errors.Add(nameof(submissionRequestData.AssignmentNumber), new Dictionary<string, string> { { "AssignmentNumber", "This assignment file has been submitted and cannot be resubmitted" } });
        // }
        
        if (assigmentMetaDataDictionary && 
            currentFileSubmissionData.ContainsKey(submissionRequestData.AssignmentNumber) && 
            !currentFileSubmissionData[submissionRequestData.AssignmentNumber].CanBeResubmitted)
        {
            errors.Add(nameof(submissionRequestData.AssignmentNumber), new Dictionary<string, string> 
            { 
                { "AssignmentNumber", "This assignment file has been submitted and cannot be resubmitted" } 
            });
        }
        
        return errors;
    }

    private static Dictionary<string, SubmissionDataModel> GetSubmissionDataDictionaryByType(
        AssigmentType assignmentType)
    {
        return assignmentType switch
        {
            AssigmentType.C => CSubmissionData,
            AssigmentType.Python => PythonSubmissionData,
            AssigmentType.Java => JavaSubmissionData,
            _ => throw new ArgumentOutOfRangeException(nameof(assignmentType), "Invalid Assignment Type")
        };
    }

    private static string GetSubmissionDataFilePathByType(AssigmentType assignmentType)
    {
        return assignmentType switch
        {
            AssigmentType.C => _cDataPath,
            AssigmentType.Python => _pythongDataPath,
            AssigmentType.Java => _javaDataPath,
            _ => throw new ArgumentOutOfRangeException(nameof(assignmentType), "Invalid Assignment Type")
        };
    }

    //public endpoint to get current submitted assignments data

    public static IResult GetCurrentSubmittedAssignmentsData(string studentId, AssigmentType assignmentType)
    {
        var data = assignmentType switch
        {
            AssigmentType.C => CSubmissionData.TryGetValue(studentId, out var cData) ? cData : null,
            AssigmentType.Python => PythonSubmissionData.TryGetValue(studentId, out var pythonData) ? pythonData : null,
            AssigmentType.Java => JavaSubmissionData.TryGetValue(studentId, out var javaData) ? javaData : null,
            _ => null
        };

        if (data == null)
        {
            _dataActionsLogger.LogInformation($"student data with id {studentId} Not Found");
            return Results.Json(new
            {
                Status = FileSubmissionStatus.Failed.ToString(),
                Message = "Student Data Not Found"
            });
        }

        _dataActionsLogger.LogInformation($"student data with id {studentId} Found");
        return Results.Json(new
        {
            Data = new SubmissionDataModel
            {
                StudentId = data.StudentId,
                Department = data.Department,
                SubmittedAssignmentCodeFiles = data.SubmittedAssignmentCodeFiles,
                SubmittedAssignmentVideoFiles = data.SubmittedAssignmentVideoFiles
            },
            Status = FileSubmissionStatus.Successfull.ToString(),
            Message = "Student Data Found"
        });
    }

    //public endpoint actions for file submissions

//     public static async Task<IResult> SubmitAssignmentFile(FileSubmissionRequestModel submissionRequestData)
//     {
//         
//         Console.WriteLine($"filled here {submissionRequestData.AssignmentType}");
//         
//         
//         var validationErrors = ValidateSubmissionRequestData(submissionRequestData);
//         if (validationErrors.Any())
//         {
//             return Results.Json(new
//             {
//                 Status = FileSubmissionStatus.Failed.ToString(), Errors = validationErrors,
//                 Message = "Validation Errors"
//             });
//         }
//
//         var submissionDataDictionary = GetSubmissionDataDictionaryByType(submissionRequestData.AssignmentType);
//         var submissionDataFilepath = GetSubmissionDataFilePathByType(submissionRequestData.AssignmentType);
//         var result = await submissionRequestData.SaveFileToStorage();
//         
//         Console.WriteLine(result);
//
//         if (!result)
//         {
//             return Results.Json(new
//                 { Status = FileSubmissionStatus.Failed, Message = "An Error Occured while Uploading File" });
//         }
//
//         var data = submissionDataDictionary.TryGetValue(submissionRequestData.StudentId,
//             out var currentGeneralData);
//
//         if (!data)
//         {
//             _fileActionsLogger.LogInformation(
//                 $"submiting {submissionRequestData.AssignmentType} {submissionRequestData.SubmissionFileType.ToString()} file for student with id {submissionRequestData.StudentId} assignment number {submissionRequestData.AssignmentNumber}");
//             var model = submissionRequestData.GenerateAddAssignmentSubmissionRequestDataModel();
//             var status =
//                 await model.AddAssignmentSubmissionDataToFile(submissionDataFilepath,
//                     submissionRequestData.SubmissionFileType);
//             if (!status)
//             {
//                 var (filePath, _) = ComputeSubmissionFilePath(submissionRequestData);
//                 TryDeleteFile(filePath);
//                 return Results.Json(new
//                     { Status = FileSubmissionStatus.Failed, Message = "An Error Occured while Uploading File" });
//             }
//
//             submissionDataDictionary.Add(submissionRequestData.StudentId, model);
//             return Results.Json(new
//                 { Status = FileSubmissionStatus.Successfull, Message = "File Successfully Uploaded" });
//         }
//
//         var updatedGeneralData =
//             currentGeneralData.GenerateUpdateAssignmentSubmissionRequestDataModel(
//                 submissionRequestData.AssignmentNumber,
//                 submissionRequestData.SubmissionFileType);
//
//         var updateResultStatus =
//             await updatedGeneralData.UpdateAssignmentSubmissionDataToFile(submissionDataFilepath,
//                 submissionRequestData.SubmissionFileType);
//
//         if (!updateResultStatus)
//         {
//             var (filePath, _) = ComputeSubmissionFilePath(submissionRequestData);
//             TryDeleteFile(filePath);
//             return Results.Json(new
//                 { Status = FileSubmissionStatus.Failed, Message = "An Error Occured while Uploading File" });
//         }
//
//         submissionRequestData.UpdateAssignmentSubmissionDataToDictionary(submissionDataDictionary);
//         return Results.Json(new { Status = FileSubmissionStatus.Successfull, Message = "File Successfully Uploaded" });
//     }

public static async Task<IResult> SubmitAssignmentFile(FileSubmissionRequestModel submissionRequestData)
{
    Console.WriteLine($"filled here {submissionRequestData.AssignmentType}");
    
    // Validation already checks resubmission limits
    var validationErrors = ValidateSubmissionRequestData(submissionRequestData);
    if (validationErrors.Any())
    {
        return Results.BadRequest(new
        {
            Status = FileSubmissionStatus.Failed.ToString(), 
            Errors = validationErrors,
            Message = "Validation Errors"
        });
    }

    var submissionDataDictionary = GetSubmissionDataDictionaryByType(submissionRequestData.AssignmentType);
    var submissionDataFilepath = GetSubmissionDataFilePathByType(submissionRequestData.AssignmentType);
    
    // Save file
    var result = await submissionRequestData.SaveFileToStorage();
    Console.WriteLine(result);

    if (!result)
    {
        return Results.InternalServerError(new
        { 
            Status = FileSubmissionStatus.Failed, 
            Message = "An Error Occurred while Uploading File" 
        });
    }

    try {
        // Get file path for potential deletion later
        var (filePath, _) = ComputeSubmissionFilePath(submissionRequestData);
        
        var data = submissionDataDictionary.TryGetValue(submissionRequestData.StudentId,
            out var currentGeneralData);

        if (!data)
        {
            _fileActionsLogger.LogInformation(
                $"submitting {submissionRequestData.AssignmentType} {submissionRequestData.SubmissionFileType.ToString()} file for student with id {submissionRequestData.StudentId} assignment number {submissionRequestData.AssignmentNumber}");
            
            var model = submissionRequestData.GenerateAddAssignmentSubmissionRequestDataModel();
            var status = await model.AddAssignmentSubmissionDataToFile(submissionDataFilepath,
                submissionRequestData.SubmissionFileType);
                
            if (!status)
            {
                TryDeleteFile(filePath);
                return Results.Json(new
                { 
                    Status = FileSubmissionStatus.Failed, 
                    Message = "An Error Occurred while Updating Submission Data" 
                });
            }

            submissionDataDictionary.Add(submissionRequestData.StudentId, model);
            return Results.Json(new
            { 
                Status = FileSubmissionStatus.Successfull, 
                Message = "File Successfully Uploaded" 
            });
        }

        var updatedGeneralData = currentGeneralData.GenerateUpdateAssignmentSubmissionRequestDataModel(
            submissionRequestData.AssignmentNumber,
            submissionRequestData.SubmissionFileType);

        var updateResultStatus = await updatedGeneralData.UpdateAssignmentSubmissionDataToFile(
            submissionDataFilepath,
            submissionRequestData.SubmissionFileType);

        if (!updateResultStatus)
        {
            TryDeleteFile(filePath);
            return Results.InternalServerError(new
            { 
                Status = FileSubmissionStatus.Failed, 
                Message = "An Error Occurred while Updating Submission Data" 
            });
        }

        submissionRequestData.UpdateAssignmentSubmissionDataToDictionary(submissionDataDictionary);
        return Results.Ok(new 
        { 
            Status = FileSubmissionStatus.Successfull, 
            Message = "File Successfully Uploaded" 
        });
    }
    catch (Exception ex)
    {
        // If any exception occurs, delete the file and log the error
        var (filePath, _) = ComputeSubmissionFilePath(submissionRequestData);
        TryDeleteFile(filePath);
        _fileActionsLogger.LogError(ex, "Error processing submission");
        
        return Results.InternalServerError(new
        { 
            Status = FileSubmissionStatus.Failed, 
            Message = "An unexpected error occurred during submission" 
        });
    }
}
}