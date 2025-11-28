namespace ApplicationLayer.Dtos
{
    public class CastMemberDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string? Bio { get; set; }
        public int TmdbId { get; set; }
    }
}
