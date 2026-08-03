using SmartCourt.Common.Entities;
﻿using SmartCourt.Common;

namespace SmartCourt.Entities
{
    public class StoredFile : BaseEntity
    {
        public Guid Id { get; set; }
        public string StoredFileName { get; set; } = null!;
        public string OriginalFileName { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public string Extension { get; set; } = null!;
        public long SizeInBytes { get; set; }
    }
}
