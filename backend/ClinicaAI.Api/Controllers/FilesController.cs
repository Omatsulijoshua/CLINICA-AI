using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicaAI.Infrastructure.Storage;

namespace ClinicaAI.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/files")]
    public class FilesController : ControllerBase
    {
        private readonly MinIoStorageService _storageService;

        public FilesController(MinIoStorageService storageService)
        {
            _storageService = storageService;
        }

        [HttpGet("download/{uniqueFileName}")]
        public async Task<IActionResult> DownloadFile(string uniqueFileName)
        {
            try
            {
                var fileStream = await _storageService.DownloadFileAsync(uniqueFileName);
                
                var contentType = "application/octet-stream";
                if (uniqueFileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    contentType = "application/pdf";
                }
                else if (uniqueFileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    contentType = "image/png";
                }
                else if (uniqueFileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || uniqueFileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    contentType = "image/jpeg";
                }
                else if (uniqueFileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
                {
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                }

                return File(fileStream, contentType, uniqueFileName);
            }
            catch (FileNotFoundException)
            {
                return NotFound("File not found in storage.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server download error: {ex.Message}");
            }
        }
    }
}
