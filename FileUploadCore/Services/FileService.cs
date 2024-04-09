using FileUploadCore.Data;
using FileUploadCore.Entities;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FileUploadCore.Services
{
    public class FileService : IFileService
    {

        private readonly DbContextClass dbContextClass;
        private readonly ILog _logger;

        public static void SaveByteArrayToFileWithBinaryWriter(byte[] data, string filePath)
        {
            using var writer = new BinaryWriter(File.OpenWrite(filePath));
            writer.Write(data);
        }

        public FileService(DbContextClass dbContextClass, ILog logger)
        {
            this.dbContextClass = dbContextClass;
            _logger = logger;
        }

        public  FileDetails DownloadFileById(int Id)
        {
           
          return  dbContextClass.FileDetails.Where(x => x.ID == Id).FirstOrDefault();
             
            
        }


        [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue)]
        public async Task PostFileAsync(IFormFile fileData)
        {
            try
            {
                var fileDetails = new FileDetails()
                {
                    ID = 0,
                    FileName = fileData.FileName,
                    FileType = fileData.FileName.Split('.')[1].ToString(),
                    ContentType = fileData.ContentType

                    
                };

                using (var stream = new MemoryStream())
                {
                    fileData.CopyTo(stream);
                    fileDetails.FileData = stream.ToArray();
                }

                var result = dbContextClass.FileDetails.Add(fileDetails);
                await dbContextClass.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public async Task PostMultiFileAsync(List<FileDetails> fileData)
        //{
        //    try
        //    {
        //        foreach (FileDetails file in fileData)
        //        {
        //            var fileDetails = new FileDetails()
        //            {
        //                ID = 0,
        //                FileName = file.FileName,
        //                FileType = file.FileType,
        //            };

        //            using (var stream = new MemoryStream())
        //            {
        //                file.FileDetails.CopyTo(stream);
        //                fileDetails.FileData = stream.ToArray();
        //            }

        //            var result = dbContextClass.FileDetails.Add(fileDetails);
        //        }
        //        await dbContextClass.SaveChangesAsync();
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

       
        async Task SaveFileWithCustomFileName(IFormFile file, string fileSaveName)
        {
            var filePath = Path.Combine(
                   Directory.GetCurrentDirectory(), "FileDownloaded",
                   file.FileName);
            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);
        }
        public async Task CopyStream(Stream stream, string downloadPath)
        {
            try
            {
                using (var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {

                _logger.Info("CopyStream : " + ex.Message); ;
            }

        }

        public Task GetAllFiles()
        {
            return dbContextClass.FileDetails.ToListAsync();
        }

      
    }
}

