using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace SmartCourt.Features.Case.AddCaseDocument.DTOs;

public class AddCaseDocumentRequest
{
    public List<IFormFile> Documents { get; set; } = new();
}
