using ApplicationLayer.Dtos;

namespace Presentation.ExtensionMethods;

public static class ToIFormFileExtension
{
    public static FileUploadDto ToFileUploadDto(this IFormFile formFile)
    {
        return new FileUploadDto
        {
            Content = formFile.OpenReadStream(),
            FileName = formFile.FileName,
            ContentType = formFile.ContentType,
            Length = formFile.Length
        };
    }
}