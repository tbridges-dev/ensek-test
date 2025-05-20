using MeterReadings.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MeterReadings.Api.Controllers;

[ApiController]
public class MeterReadingController(ILogger<MeterReadingController> logger, IMeterReadingService meterReadingService) : ControllerBase
{
    private readonly ILogger<MeterReadingController> _logger = logger;
    private readonly IMeterReadingService _meterReadingService = meterReadingService;

    [Route("meter-reading-uploads")]
    [HttpPost]
    public async Task<IActionResult> UploadMeterReadings(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogError("File uploaded for processing was either missing or empty");
            ModelState.AddModelError("FileUploadError", "File was not provided or empty.");
            return BadRequest(ModelState);
        }

        var meterReadingProcessResponse = await _meterReadingService.ProcessMeterReadingsAsync(file.OpenReadStream(), cancellationToken);

        return Ok(meterReadingProcessResponse);
    }
}
