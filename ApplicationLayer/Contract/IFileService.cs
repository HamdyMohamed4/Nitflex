using ApplicationLayer.Dtos;

namespace ApplicationLayer.Contract;

public interface IFileService
{
    Task<string> UplaodProfilePictureAsync(FileUploadDto file);
    Task DeleteProfilePictureAsync(string path);
}