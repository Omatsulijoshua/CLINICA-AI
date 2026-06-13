using System;

namespace ClinicaAI.Core.Entities
{
    public class VideoReference
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid MessageId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string DurationString { get; set; } = string.Empty;
        public string ChannelName { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;

        // Navigation Properties
        public Message Message { get; set; } = null!;
    }
}
