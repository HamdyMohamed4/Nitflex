namespace ApplicationLayer.Dtos;

public class FileUploadDto
{
    public Stream Content { get; set; } = default!;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Length { get; set; }
}