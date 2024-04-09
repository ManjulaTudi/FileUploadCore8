using FileUploadCore.Data;
using FileUploadCore.Entities;
using FileUploadCore.Models;
using FileUploadCore.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;

namespace FileUploadCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {

        private readonly IFileService _fileUploasService;
        private readonly ILog _logger;
        private readonly DbContextClass dbContextClass;
        private readonly IConfiguration _configuration;
        public FileController(IFileService fileService, ILog logger, DbContextClass dbContextClass, IConfiguration config)
        {
            _fileUploasService = fileService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dbContextClass = dbContextClass;
            _configuration = (IConfigurationRoot)config ?? throw new ArgumentNullException(nameof(config));

        }
        [DisableRequestSizeLimit]
        [HttpPost("PostSingleFile")]
        public async Task<ActionResult> PostSingleFile(IFormFile file)
        {

            if (file == null)
            {
                _logger.Error("file is null");
                return BadRequest();
            }

            try
            {
                var AllowFileUpload = Convert.ToBoolean(_configuration.GetSection("FileUpload:AllowFileUpload").Value);

                if (AllowFileUpload)
                {
                    _logger.Info(string.Format("file '{0}'  upload  is in progress", file.Name.ToString()));
                    await _fileUploasService.PostFileAsync(file);
                    return Ok();
                }
                else
                {
                    BadRequest("file upload is not allowed");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }
            return BadRequest();
        }


        [HttpPost("DownloadFile")]
        public IActionResult Download(int id)
        {
            TimeSpan start = new TimeSpan(8, 0, 0);
            TimeSpan end = new TimeSpan(14, 0, 0);
            var AllowFileDownload = Convert.ToBoolean(_configuration.GetSection("FileUpload:AllowFileDowload").Value);
            if (DateTime.UtcNow.Hour > start.Hours && DateTime.UtcNow.Hour < end.Hours && AllowFileDownload)
            {

                if (id < 1)
                {
                    return BadRequest();
                }

                byte[] bytes;
                string fileName, contentType;

                var item = dbContextClass.FileDetails.FirstOrDefault(c => c.ID == id);

                if (item != null)
                {
                    fileName = item.FileName;
                    contentType = item.ContentType;
                    bytes = item.FileData;

                    return File(bytes, contentType, fileName);
                }
                else
                {
                    return BadRequest("file not exist");
                }
            }
            return BadRequest("file download is not allowed");

        }

        [HttpGet("GetAllFiles")]
        public IActionResult GetFiles()
        {
            var files = dbContextClass.FileDetails
                .Select(x => new FileDetails
                {
                    FileName = x.FileName,
                    ContentType = x.ContentType,
                    ID = x.ID
                }).ToList();

            return Ok(files);
        }

    }


}
