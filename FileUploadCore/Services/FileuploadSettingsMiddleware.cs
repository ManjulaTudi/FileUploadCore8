using log4net;
using System.Linq;
using System.Net.Mime;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Collections.Generic;
using System.Drawing;

namespace FileUploadCore.Services
{
    public class FileuploadSettingsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILog _logger;
        private readonly IConfiguration _configuration;
        public FileuploadSettingsMiddleware(RequestDelegate next, ILog logger, IConfiguration config)
        {
            _next = next;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = (IConfigurationRoot)config ?? throw new ArgumentNullException(nameof(config));
        }
        public async Task InvokeAsync(HttpContext context)
        {

            try
            {
                List<ErrorModel> errors = new List<ErrorModel>();

                // Perform authentication logic
                if (context.Request.ContentType != null)
                {
                    if (context.Request.Form.Files.Count > 0)
                    {
                        IFormFileCollection files = context.Request.Form.Files;
                        errors = fileValidations(files);
                        if (errors.Count >= 1)
                        {
                            HandleCustomExceptionResponseAsync(context, errors);

                        }
                        else
                        {
                            _logger.Info("file details " + context.Request.Form.Files[0].FileName);
                            await _next(context);
                        }
                    }

                }
                else
                {
                    await _next(context);
                }
            }
            catch (Exception ex)
            {

                _logger.Info(string.Format("File middleware  message : {0} : stack trace : {1}", ex.Message, ex.StackTrace));
            }
        }

        private List<ErrorModel> fileValidations(IFormFileCollection files)
        {
            List<ErrorModel> errors = new List<ErrorModel>();

            var fileExtentions = _configuration.GetSection("FileUpload:FileExtentions").Value;
            var fileextList = new List<string>();
            if (fileExtentions != string.Empty)
            {
                fileextList= fileExtentions.Split(',').ToList();
            }
            //?? _configuration.GetSection("FileUpload:FileExtentions").Value.Split(',').ToArray(),
            fileUploadSettings fileUploadSettings = new fileUploadSettings
            {
                FileSizeLimit = Convert.ToInt64(_configuration.GetSection("FileUpload:FileSizeLimit").Value),
                AllowFileUpload = Convert.ToBoolean(_configuration.GetSection("FileUpload:AllowFileUpload").Value),
                AllowFileDowload = Convert.ToBoolean(_configuration.GetSection("FileUpload:AllowFileDowload").Value),
                FileExtentions = fileExtentions
            };
            Parallel.ForEach(files, x =>
                {
                    if (!IsValidFileName(x.FileName))
                    {
                        errors.Add(new ErrorModel
                        {
                            Error = "file name",
                            Message = "Invalid file name"

                        });
                        _logger.Info(string.Format("file content :{0}, file name :{1}", x.ContentType, x.FileName));
                    }
                });
            var notValidfileList = files.Where(x => !fileExtentions.Contains(x.FileName.Split('.')[1].ToString())).ToList();

            Parallel.ForEach(notValidfileList, x =>
         {
             errors.Add(new ErrorModel
             {
                 Error = "Content type",
                 Message = x.ContentType.ToString() + " is not a valid content type"

             });
             _logger.Info(string.Format("file content :{0}, file name :{1}", x.ContentType, x.FileName));
         });

            Parallel.ForEach(files.Where(x => x.Length > fileUploadSettings.FileSizeLimit).ToList(), x =>
        {
            errors.Add(new ErrorModel
            {
                Error = "File size",
                Message = "File size exceeded - " + x.Length.ToString()

            });
            _logger.Info(string.Format("file content :{0}, file name :{1}: File size ;{2}",
                x.ContentType, x.FileName,
               x.Length));

        });

            Parallel.ForEach(files.Where(x => x.Length > 0).ToList(), x =>
        {
            using (var ms = new MemoryStream())
            {
                x.CopyTo(ms);
                var fileBytes = ms.ToArray();

                if (!CheckForValidFileType(fileBytes, x.ContentType))
                    errors.Add(new ErrorModel
                    {
                        Error = "Invalid file type",
                        Message = x.ContentType.ToString() + ": invalid file selected"

                    });
                _logger.Info(string.Format("file content :{0}, file name :{1}", x.ContentType, x.FileName));
            }
        });

            return errors;
        }
        private async Task HandleCustomExceptionResponseAsync(HttpContext context, List<ErrorModel> errors)
        {

            context.Response.ContentType = MediaTypeNames.Application.Json;
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            var json = JsonSerializer.Serialize(string.Join(",", errors.Select(x => $"'{x.Error} : {x.Message}'")), options);
            await context.Response.WriteAsync(json);
            TypedResults.BadRequest(json);
        }
        private bool IsValidFileName(string fileName)
        {
            // Define a whitelist of valid characters.
            // allows only alphanumeric characters, underscores, dots, and hyphens in the file name.

            Regex containsABadCharacter = new Regex("["
           + Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()) + "]"));
            if (containsABadCharacter.IsMatch(fileName)) { return false; };

            return true;
        }
        private static bool CheckForValidFileType(byte[] data, string contentType)
        {
            var text = ASCIIEncoding.ASCII.GetString(data);
            bool isValidFile = true;
            switch (contentType)
            {

                case "application/pdf":
                    if (!text.StartsWith("%PDF"))
                        isValidFile = false;
                    break;


                default:
                    isValidFile = true;
                    break;
            }
            return isValidFile;
        }
    }

    public static class FileuploadSettingsMiddlewareExtensions
    {
        public static IApplicationBuilder UseFileUploadValidator(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FileuploadSettingsMiddleware>();
        }
    }


    class fileUploadSettings
    {
        public Int64 FileSizeLimit { get; set; }
        public string FileExtentions { get; set; }
        public bool AllowFileUpload { get; set; }
        public bool AllowFileDowload { get; set; }

    }
    class ErrorModel
    {
        public string Error { get; set; }
        public string Message { get; set; }
    }
}