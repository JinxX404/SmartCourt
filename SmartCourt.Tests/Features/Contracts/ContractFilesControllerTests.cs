using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Features.Contracts.Files;
using SmartCourt.Features.Contracts.Validators;
using Xunit;

namespace SmartCourt.Tests.Features.Contracts;

public sealed class ContractFilesControllerTests
{
    [Fact]
    public async Task Upload_ReturnsCreatedWrappedMetadata()
    {
        var service = new RecordingContractFilePort();
        var controller = CreateController(service);
        var contractId = Guid.NewGuid();
        var request = new UploadContractFilesRequest
        {
            Files =
            [
                CreateFormFile(
                    "evidence.pdf",
                    "%PDF-1.7\nevidence"u8.ToArray())
            ]
        };

        var action = await controller.UploadAsync(
            contractId,
            request,
            CancellationToken.None);

        var result = Assert.IsType<ObjectResult>(ConvertAction(action));
        var response = Assert.IsType<
            ApiResponse<IReadOnlyList<ContractFileDto>>>(result.Value);
        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.True(response.Success);
        Assert.Same(service.UploadedFiles, response.Data);
        Assert.Equal(contractId, service.UploadContractId);
        Assert.Same(request, service.UploadRequest);
    }

    [Fact]
    public async Task Upload_InvalidRequestFailsBeforeServiceCall()
    {
        var service = new RecordingContractFilePort();
        var controller = CreateController(service);

        await Assert.ThrowsAsync<BusinessException>(() =>
            controller.UploadAsync(
                Guid.NewGuid(),
                new UploadContractFilesRequest(),
                CancellationToken.None));

        Assert.Equal(0, service.UploadCallCount);
    }

    [Fact]
    public async Task Delete_ReturnsWrappedOkAndForwardsIdentifiers()
    {
        var service = new RecordingContractFilePort();
        var controller = CreateController(service);
        var contractId = Guid.NewGuid();
        var storedFileId = Guid.NewGuid();

        var action = await controller.DeleteAsync(
            contractId,
            storedFileId,
            CancellationToken.None);

        var result = Assert.IsType<OkObjectResult>(ConvertAction(action));
        var response = Assert.IsType<ApiResponse>(result.Value);
        Assert.True(response.Success);
        Assert.Equal(contractId, service.DeleteContractId);
        Assert.Equal(storedFileId, service.DeleteStoredFileId);
    }

    [Fact]
    public void Endpoints_DefineContractScopedRoutesAndParticipantRoles()
    {
        var controllerRoute = Assert.Single(
            typeof(ContractFilesController)
                .GetCustomAttributes(typeof(RouteAttribute), inherit: true)
                .Cast<RouteAttribute>());
        Assert.Equal(
            "api/contracts/{contractId:guid}/files",
            controllerRoute.Template);
        var authorize = Assert.Single(
            typeof(ContractFilesController)
                .GetCustomAttributes<AuthorizeAttribute>(inherit: true));
        Assert.Equal("Client,Lawyer", authorize.Roles);

        AssertEndpoint(
            nameof(ContractFilesController.UploadAsync),
            typeof(HttpPostAttribute),
            null);
        AssertEndpoint(
            nameof(ContractFilesController.DeleteAsync),
            typeof(HttpDeleteAttribute),
            "{storedFileId:guid}");
    }

    [Fact]
    public async Task Validator_EnforcesFileCountAndSizeLimits()
    {
        var validator = new UploadContractFilesRequestValidator();
        var tooMany = new UploadContractFilesRequest
        {
            Files = Enumerable.Range(0, 6)
                .Select(index => CreateFormFile(
                    $"file-{index}.txt",
                    "content"u8.ToArray()))
                .ToArray()
        };
        var tooLarge = new UploadContractFilesRequest
        {
            Files =
            [
                CreateSizedFormFile(
                    "large.pdf",
                    ContractFileUploadPolicy.MaximumFileSizeBytes + 1)
            ]
        };

        var tooManyResult = await validator.ValidateAsync(tooMany);
        var tooLargeResult = await validator.ValidateAsync(tooLarge);

        Assert.False(tooManyResult.IsValid);
        Assert.False(tooLargeResult.IsValid);
    }

    private static ContractFilesController CreateController(
        RecordingContractFilePort service)
    {
        return new ContractFilesController(
            service,
            new UploadContractFilesRequestValidator());
    }

    private static void AssertEndpoint(
        string methodName,
        Type httpAttributeType,
        string? route)
    {
        var method = typeof(ContractFilesController).GetMethod(methodName);
        Assert.NotNull(method);
        var httpAttribute = Assert.Single(
            method.GetCustomAttributes(httpAttributeType, inherit: true));
        var template = Assert.IsAssignableFrom<HttpMethodAttribute>(
            httpAttribute).Template;
        Assert.Equal(route, template);
    }

    private static IActionResult ConvertAction<T>(ActionResult<T> action)
    {
        return ((IConvertToActionResult)action).Convert();
    }

    private static IFormFile CreateFormFile(
        string fileName,
        byte[] content)
    {
        return new FormFile(
            new MemoryStream(content),
            0,
            content.Length,
            "Files",
            fileName);
    }

    private static IFormFile CreateSizedFormFile(
        string fileName,
        long size)
    {
        return new FormFile(
            Stream.Null,
            0,
            size,
            "Files",
            fileName);
    }

    private sealed class RecordingContractFilePort : IContractFileService
    {
        public IReadOnlyList<ContractFileDto> UploadedFiles { get; } =
        [
            new ContractFileDto(
                Guid.NewGuid(),
                "evidence.pdf",
                "application/pdf",
                20,
                new DateTimeOffset(
                    2026,
                    8,
                    16,
                    10,
                    0,
                    0,
                    TimeSpan.Zero))
        ];

        public int UploadCallCount { get; private set; }
        public Guid? UploadContractId { get; private set; }
        public UploadContractFilesRequest? UploadRequest { get; private set; }
        public Guid? DeleteContractId { get; private set; }
        public Guid? DeleteStoredFileId { get; private set; }

        public Task<IReadOnlyList<ContractFileDto>> UploadAsync(
            Guid contractId,
            UploadContractFilesRequest request,
            CancellationToken cancellationToken)
        {
            UploadCallCount++;
            UploadContractId = contractId;
            UploadRequest = request;
            return Task.FromResult(UploadedFiles);
        }

        public Task DeleteAsync(
            Guid contractId,
            Guid storedFileId,
            CancellationToken cancellationToken)
        {
            DeleteContractId = contractId;
            DeleteStoredFileId = storedFileId;
            return Task.CompletedTask;
        }
    }
}
