using SmartCourt.Common.Entities;
﻿using SmartCourt.Common;

namespace SmartCourt.Entities
{
    public class StoredFile : BaseEntity
    {
        public Guid Id { get; set; }
        public string StoredFileName { get; set; }
        public string OriginalFileName { get; set; }
        public string FileUrl { get; set; }
        public string ContentType { get; set; }
        public string Extension { get; set; }
        public long SizeInBytes { get; set; }
    }
}
