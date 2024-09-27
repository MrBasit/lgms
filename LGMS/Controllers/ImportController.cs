using LGMS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly ExcelImportService _excelService;

        public ImportController(ExcelImportService excelService)
        {
            _excelService = excelService;
        }

        [HttpPost("UploadEquipments")]
        public async Task<IActionResult> UploadEquipments(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file.");

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    var equipments = _excelService.ParseEquipmentExcelFile(stream);

                    return Ok(new { message = $"{equipments.Count} records processed successfully." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
