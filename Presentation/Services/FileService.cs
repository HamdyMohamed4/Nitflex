using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;

namespace Presentation.Services;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _env;

    public FileService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> UplaodProfilePictureAsync(FileUploadDto file)
    {
        string profilePictureNameWithFileExtension = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        string relativePath = Path.Combine("uploads", "profiles", profilePictureNameWithFileExtension);

        string profilePicturePath = Path.Combine(_env.WebRootPath, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(profilePicturePath)!);

        using (var stream = new FileStream(profilePicturePath, FileMode.Create))
        {
            await file.Content.CopyToAsync(stream);
        }

        return profilePictureNameWithFileExtension;
    }

    public Task DeleteProfilePictureAsync(string path)
    {
        var profilePicturePath = Path.Combine(_env.WebRootPath, path.Trim());

        if (File.Exists(profilePicturePath))
        {
            File.Delete(profilePicturePath);
        }

        return Task.CompletedTask;
    }
}