using FileUploadCore.Entities;
using Microsoft.VisualBasic.FileIO;

namespace FileUploadCore.Services
{
    public interface IFileService
    {
        public Task PostFileAsync(IFormFile fileData);

    }
}
