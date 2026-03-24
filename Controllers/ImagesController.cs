using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MisureRicci.Services;

namespace MisureRicci.Controllers
{
    [Authorize]
    [Route("images")]
    public class ImagesController : Controller
    {
        private readonly IMeasurementTypeImageStorageService _measurementTypeImageStorageService;

        public ImagesController(IMeasurementTypeImageStorageService measurementTypeImageStorageService)
        {
            _measurementTypeImageStorageService = measurementTypeImageStorageService;
        }

        [HttpGet("measurement-types/{fileName}")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult MeasurementType(string fileName)
        {
            var image = _measurementTypeImageStorageService.GetImage(fileName);
            if (image == null)
            {
                return NotFound();
            }

            Response.Headers.CacheControl = "no-store, no-cache, max-age=0";
            Response.Headers.Pragma = "no-cache";

            return PhysicalFile(image.PhysicalPath, image.ContentType);
        }
    }
}
