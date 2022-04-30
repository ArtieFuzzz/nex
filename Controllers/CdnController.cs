using Microsoft.AspNetCore.Mvc;
using filesys = System.IO.File;
using System.Text.RegularExpressions;
using JWT.Builder;
using JWT.Algorithms;

namespace nex.Controllers;

[ApiController]
[Route("i")]
[ResponseCache(Duration = 86400, NoStore = false, Location = ResponseCacheLocation.Client)]
public class CdnController : ControllerBase
{
  private string ASSET_LOCATION = "./assets";
  private string JWT_SECRET = Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new Exception("JWT_SECRET environment variable not set");

  [HttpGet("{name}")]
  public IActionResult GetFile(String name)
  {
    var pathToFile = $"{ASSET_LOCATION}/{name}";

    if (!filesys.Exists(pathToFile))
    {
      return NotFound("File Doesn't Exist");
    }

    var file = filesys.OpenRead(pathToFile);
    var meta = new FileInfo(pathToFile);
    // This Regex took me some time to get right... Lol
    var ext = Regex.Replace(meta.Extension, @"(\.)", " ").Trim();

    return File(file, $"image/{ext}");
  }

  [HttpPost("upload")]
  public async Task<IActionResult> PostFile([FromForm] IFormFile file)
  {
    if (Request.Headers["Authorization"].Count() == 0) return Unauthorized("No Authorization");
    
    if (!VerifyJWT()) return Unauthorized("Invalid Token");

    if (!(file.Length <= 0))
    {
      var imageExts = new List<String> { "jpg", "jpeg", "png", "gif" };
      var fileExt = Path.GetExtension(file.FileName).Substring(1);

      if (!imageExts.Contains(fileExt))
      {
        return BadRequest("Unallowed File Extention");
      }

      if (filesys.Exists($"{ASSET_LOCATION}/{file.FileName}"))
      {
        return Ok("File Already Exists");
      }

      using (var stream = System.IO.File.Create($"{ASSET_LOCATION}/{file.FileName}"))
      {
        await file.CopyToAsync(stream);
      }

      return Ok("File Successfully Uploaded");
    }

    return BadRequest("Invalid");
  }

  private bool VerifyJWT()
  {
    try
    {
      JwtBuilder.Create()
        .WithAlgorithm(new HMACSHA256Algorithm()) // symmetric
        .WithSecret(JWT_SECRET)
        .MustVerifySignature()
        .Decode(Request.Headers["Authorization"]);
    }
    catch
    {
      return false;
    }

    return true;
  }
}
