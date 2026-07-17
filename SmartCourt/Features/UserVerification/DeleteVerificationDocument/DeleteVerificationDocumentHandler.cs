using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common;
using SmartCourt.Common.Enums;
using SmartCourt.Entities;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;

namespace SmartCourt.Features.UserVerification.DeleteVerificationDocument
{
    public sealed class DeleteVerificationDocumentCommandHandler : IRequestHandler<DeleteVerificationDocumentCommand, ApiResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileStorageService _fileStorageService;
        private readonly IValidator<DeleteVerificationDocumentCommand> _validator;

        public DeleteVerificationDocumentCommandHandler(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IFileStorageService fileStorageService,
            IValidator<DeleteVerificationDocumentCommand> validator)
        {
            _context = context;
            _userManager = userManager;
            _fileStorageService = fileStorageService;
            _validator = validator;
        }

        public async Task<ApiResponse> Handle(
            DeleteVerificationDocumentCommand request,
            CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);

            if (!validationResult.IsValid)
                return ApiResponse.Fail(validationResult.Errors.Select(x => x.ErrorMessage).ToList(), 400);

            var user = await _userManager.FindByIdAsync(request.UserId);

            if (user is null)
                return ApiResponse.Fail(new List<string>{ "The specified user doesn't exist." }, 404);

            var document = await _context.UserVerificationDocuments
                .Where(d => d.UserId == request.UserId && d.StoredFile.Id == request.DocumentId)
                .Include(d => d.StoredFile)
                .FirstOrDefaultAsync();

            if (document is null)
                return ApiResponse.Fail(new List<string>{ "Verification document was not found." }, 404);

            if (document.Status == VerificationDocumentStatus.Verified)
            {
                //Changing the verification status of the user to be Pending,
                //untill the user upload a new document to get verified
            }

            try
            {
                await _fileStorageService.DeleteAsync(
                    document.StoredFile.FileUrl,
                    cancellationToken);
            }
            catch
            {
                return ApiResponse.Fail(new List<string>{ "An error occured while deleting the document. Try again please.." });
            }

            _context.StoredFiles.Remove(document.StoredFile);
            _context.UserVerificationDocuments.Remove(document);

            await _context.SaveChangesAsync();

            return ApiResponse.Ok();
        }
    }
}
