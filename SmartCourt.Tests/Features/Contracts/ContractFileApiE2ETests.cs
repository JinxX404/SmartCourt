using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartCourt.Common.Models;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Contracts.Files;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using SmartCourt.Tests.Common;
using Xunit;

namespace SmartCourt.Tests.Features.Contracts;

public sealed class ContractFileApiE2ETests(
    ContractFileWebApplicationFactory factory)
    : IClassFixture<ContractFileWebApplicationFactory>
{
    [Fact]
    public async Task Participant_CanUploadMultipartAndDeleteUnusedFile()
    {
        var state = await SeedContractAsync();
        var client = factory.CreateAuthenticatedClient(
            state.ClientUserId,
            "Client",
            $"client-{state.ClientUserId:N}@test.com");
        using var multipart = CreatePdfMultipart("evidence.pdf");

        var uploadResponse = await client.PostAsync(
            $"/api/contracts/{state.ContractId}/files",
            multipart);

        Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);
        var envelope = await uploadResponse.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<ContractFileDto>>>();
        Assert.NotNull(envelope);
        Assert.True(envelope.Success);
        var uploaded = Assert.Single(envelope.Data!);
        Assert.Equal("evidence.pdf", uploaded.FileName);
        Assert.Equal("application/pdf", uploaded.ContentType);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();
            var attachment = await db.ContractAttachments
                .SingleAsync(item => item.StoredFileId == uploaded.StoredFileId);
            Assert.Equal(state.ContractId, attachment.ContractId);
            Assert.Equal(state.ClientUserId, attachment.UploadedByUserId);
            Assert.False(await db.UserVerificationDocuments.AnyAsync(
                document => document.StoredFileId == uploaded.StoredFileId));
        }

        var deleteResponse = await client.DeleteAsync(
            $"/api/contracts/{state.ContractId}/files/{uploaded.StoredFileId}");

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();
            Assert.False(await db.StoredFiles.AnyAsync(
                item => item.Id == uploaded.StoredFileId));
            Assert.False(await db.ContractAttachments.AnyAsync(
                item => item.StoredFileId == uploaded.StoredFileId));
        }
        Assert.Contains(
            factory.Storage.DeletedPaths,
            path => path.Contains(
                uploaded.StoredFileId.ToString("N"),
                StringComparison.Ordinal));
    }

    [Fact]
    public async Task NonParticipant_UploadReceivesForbiddenWithoutStorageWrite()
    {
        var state = await SeedContractAsync();
        var unrelatedUserId = Guid.NewGuid();
        await factory.SeedUserAsync(
            unrelatedUserId,
            $"unrelated-{unrelatedUserId:N}@test.com",
            "Client");
        var client = factory.CreateAuthenticatedClient(
            unrelatedUserId,
            "Client",
            $"unrelated-{unrelatedUserId:N}@test.com");
        var uploadCount = factory.Storage.UploadedPaths.Count;
        using var multipart = CreatePdfMultipart("evidence.pdf");

        var response = await client.PostAsync(
            $"/api/contracts/{state.ContractId}/files",
            multipart);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(uploadCount, factory.Storage.UploadedPaths.Count);
    }

    [Fact]
    public async Task Participant_ExtensionSpoofingReceivesBadRequest()
    {
        var state = await SeedContractAsync();
        var client = factory.CreateAuthenticatedClient(
            state.LawyerUserId,
            "Lawyer",
            $"lawyer-{state.LawyerUserId:N}@test.com");
        using var multipart = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent("not a pdf"u8.ToArray());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            "application/pdf");
        multipart.Add(fileContent, "Files", "forged.pdf");

        var response = await client.PostAsync(
            $"/api/contracts/{state.ContractId}/files",
            multipart);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<ContractState> SeedContractAsync()
    {
        var lawyerUserId = Guid.NewGuid();
        var clientUserId = Guid.NewGuid();
        await factory.SeedUserAsync(
            lawyerUserId,
            $"lawyer-{lawyerUserId:N}@test.com",
            "Lawyer");
        await factory.SeedUserAsync(
            clientUserId,
            $"client-{clientUserId:N}@test.com",
            "Client");

        var contract = new Contract(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            clientUserId,
            lawyerUserId,
            "عقد اختبار رفع الملفات",
            "شروط صالحة لاختبار مسار رفع الملفات عبر HTTP.",
            DateTimeOffset.UtcNow);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        db.Contracts.Add(contract);
        await db.SaveChangesAsync();
        return new ContractState(contract.Id, clientUserId, lawyerUserId);
    }

    private static MultipartFormDataContent CreatePdfMultipart(
        string fileName)
    {
        var multipart = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(
            "%PDF-1.7\ncontract evidence"u8.ToArray());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            "application/pdf");
        multipart.Add(fileContent, "Files", fileName);
        return multipart;
    }

    private sealed record ContractState(
        Guid ContractId,
        Guid ClientUserId,
        Guid LawyerUserId);
}

public sealed class ContractFileWebApplicationFactory
    : SmartCourtWebApplicationFactory
{
    public ContractFileApiStorage Storage { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IFileStorageService>();
            services.AddSingleton<IFileStorageService>(Storage);
        });
    }
}

public sealed class ContractFileApiStorage : IFileStorageService
{
    public List<string> UploadedPaths { get; } = [];
    public List<string> DeletedPaths { get; } = [];

    public Task<FileUploadResult> UploadAsync(
        Stream stream,
        string filePath,
        string originalFileName,
        CancellationToken cancellationToken = default)
        => UploadAsync(
            stream,
            filePath,
            originalFileName,
            null,
            cancellationToken);

    public Task<FileUploadResult> UploadAsync(
        Stream stream,
        string filePath,
        string originalFileName,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        UploadedPaths.Add(filePath);
        return Task.FromResult(new FileUploadResult
        {
            StoragePath = filePath,
            OriginalFileName = originalFileName,
            Size = stream.Length
        });
    }

    public Task DeleteAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        DeletedPaths.Add(filePath);
        return Task.CompletedTask;
    }

    public Task<byte[]> DownloadAsync(
        string filePath,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<bool> ExistsAsync(
        string filePath,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<string> GetDownloadUrlAsync(
        string filePath,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}
