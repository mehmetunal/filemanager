using Dev.Core.IO;
using Dev.Core.IO.Model;
using Dev.Framework.Api;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;

namespace Dev.FileManager.Api.Controllers
{
    [Route("api/files")]
    public class FilesController : BaseApiController
    {
        private readonly IFilesManager _filesManager;
        private readonly IDevFileProvider _devFileProvider;
        private readonly IFileManagerProviderBase _fileManagerProviderBase;
        public FilesController(
            IFilesManager filesManager,
            IDevFileProvider devFileProvider,
            IFileManagerProviderBase fileManagerProviderBase)
        {
            _filesManager = filesManager;
            _devFileProvider = devFileProvider;
            _fileManagerProviderBase = fileManagerProviderBase;
        }

        // GET: api/<FilesController>
        [HttpGet()]
        public string[] Get()
        {
            var path = _devFileProvider.GetAbsolutePath("/");
            //var files = _devFileProvider.GetFiles(path, topDirectoryOnly: false);
            //string[] entries = _devFileProvider.GetFileSystemEntries(path, "*");
            var entries = _devFileProvider.PrintDirectoryTree(path);
            return entries.ToArray();
        }
    }
}
