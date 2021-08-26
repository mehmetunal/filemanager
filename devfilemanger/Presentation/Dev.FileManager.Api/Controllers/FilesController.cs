using Dev.Core.IO;
using Dev.Framework.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;

namespace Dev.FileManager.Api.Controllers
{
    [Route("api/files")]
    public class FilesController : BaseApiController
    {
        private readonly string _tempFolder;
        private readonly IFilesManager _filesManager;
        private readonly ResponseContext _responseData;
        private readonly IDevFileProvider _devFileProvider;
        private readonly string _root;
        public FilesController(
            IFilesManager filesManager,
            IDevFileProvider devFileProvider
            )
        {
            _filesManager = filesManager;
            _devFileProvider = devFileProvider;
            _responseData = new ResponseContext();
            _root = _devFileProvider.GetAbsolutePath("/");
            _tempFolder = $"{_root}/Temp";
        }

        [HttpPost("upload")]
        public IActionResult Upload(IFormFile fileToUpload, string file, int num)
        {
            try
            {
                var fileName = file + num;

                _filesManager.FolderCreat(_tempFolder);
                _filesManager.FilesCreat(_tempFolder, fileToUpload, fileName);
            }
            catch (Exception ex)
            {
                _responseData.ErrorMessage = ex.Message;
                _responseData.IsSuccess = false;
            }
            return Ok(_responseData);
        }
        [HttpPost("uploadComplete")]
        public IActionResult UploadComplete(string fileName)
        {
            try
            {
                string tempPath = _tempFolder;
                string newPath = Path.Combine(tempPath, fileName);
                string[] filePaths = _devFileProvider.GetFiles(tempPath).Where(p => p.Contains(fileName)).OrderBy(p => Int32.Parse(p.Replace(fileName, "$").Split('$')[1])).ToArray();
                foreach (string filePath in filePaths)
                {
                    _filesManager.MergeChunks(newPath, filePath);
                }
                _devFileProvider.FileMove(Path.Combine(tempPath, fileName), Path.Combine(_root, fileName));
            }
            catch (Exception ex)
            {
                _responseData.ErrorMessage = ex.Message;
                _responseData.IsSuccess = false;
            }
            return Ok(_responseData);
        }
    }
}
public class ResponseContext
{
    public dynamic Data { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string ErrorMessage { get; set; }
}
