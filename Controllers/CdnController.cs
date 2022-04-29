using Microsoft.AspNetCore.Mvc;
using filesys = System.IO.File;
using System.Text.RegularExpressions;

namespace nex.Controllers;

[ApiController]
[Route("i")]
public class CdnController : ControllerBase
{
  private string ASSET_LOCATION = "./assets";

  [HttpGet("{name}")]
  public IActionResult GetFile(String name)
  {
    var pathToFile = $"{ASSET_LOCATION}/{name}";

    if (!filesys.Exists(pathToFile))
    {
      return NotFound();
    }

    var file = filesys.OpenRead(pathToFile);
    var meta = new FileInfo(pathToFile);
    // This Regex took me some time to get right... Lol
    var ext = Regex.Replace(meta.Extension, @"(\.)", " ").Trim();

    return File(file, $"image/{ext}");
  }

  [HttpPost("upload")]
  public async Task<IActionResult> PostFile([FromForm]IFormFile file)
  {
    if (!(file.Length <= 0))
    {
      var imageExts = new List<String> { "jpg", "jpeg", "png", "gif" };
      var fileExt = Path.GetExtension(file.FileName).Substring(1);

      if (!imageExts.Contains(fileExt))
      {
        return BadRequest("Invalid File Extension");
      }

      using (var stream = System.IO.File.Create($"{ASSET_LOCATION}{file.FileName}"))
      {
        await file.CopyToAsync(stream);
      }

      return Ok("File Successfully Uploaded");
    }

    return BadRequest("Invalid File");
  }
}
