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
        private readonly ResponseContext _responseData;
        private IHostingEnvironment _environment;
        private readonly string _tempFolder;
        public FilesController(IHostingEnvironment environment)
        {
            _environment = environment;
            _responseData = new ResponseContext();
            _tempFolder = _environment.WebRootPath + "/Temp";
        }

        [HttpPost("upload")]
        public IActionResult Upload(IFormFile fileToUpload, string file, int num)
        {
            try
            {
                var chunkNumber = num;
                string newpath = Path.Combine(_tempFolder, file + chunkNumber);
                if (!Directory.Exists(_tempFolder))
                {
                    Directory.CreateDirectory(_tempFolder);
                }

                using (FileStream fs = System.IO.File.Create(newpath))
                //Create the file in your file system with the name you want.
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        //Copy the uploaded file data to a memory stream
                        fileToUpload.CopyTo(ms);
                        //Now write the data in the memory stream to the new file
                        fs.Write(ms.ToArray());
                    }
                }
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
                string[] filePaths = Directory.GetFiles(tempPath).Where(p => p.Contains(fileName)).OrderBy(p => Int32.Parse(p.Replace(fileName, "$").Split('$')[1])).ToArray();
                foreach (string filePath in filePaths)
                {
                    MergeChunks(newPath, filePath);
                }
                System.IO.File.Move(Path.Combine(tempPath, fileName), Path.Combine(_environment.WebRootPath, fileName));
            }
            catch (Exception ex)
            {
                _responseData.ErrorMessage = ex.Message;
                _responseData.IsSuccess = false;
            }
            return Ok(_responseData);
        }
        private static void MergeChunks(string chunk1, string chunk2)
        {
            FileStream fs1 = null;
            FileStream fs2 = null;
            try
            {
                fs1 = System.IO.File.Open(chunk1, FileMode.Append);
                fs2 = System.IO.File.Open(chunk2, FileMode.Open);
                byte[] fs2Content = new byte[fs2.Length];
                fs2.Read(fs2Content, 0, (int)fs2.Length);
                fs1.Write(fs2Content, 0, (int)fs2.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " : " + ex.StackTrace);
            }
            finally
            {
                if (fs1 != null) fs1.Close();
                if (fs2 != null) fs2.Close();
                System.IO.File.Delete(chunk2);
            }
        }
    }
}
public class ResponseContext
{
    public dynamic Data { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string ErrorMessage { get; set; }
}
