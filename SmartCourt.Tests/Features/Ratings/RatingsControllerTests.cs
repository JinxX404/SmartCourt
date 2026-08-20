using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.Ratings;
using SmartCourt.Features.Ratings.DTOs;
using SmartCourt.Features.Ratings.Enums;
using Xunit;

namespace SmartCourt.Tests.Features.Ratings;

public sealed class RatingsControllerTests
{
    private sealed class StubRatingService : IRatingService
    {
        public ContractRatingDto RatingToReturn { get; set; } = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Client Name",
            "Lawyer Name",
            RaterRole.Client,
            5,
            "Excellent",
            DateTime.UtcNow);

        public ContractRatingSummaryDto SummaryToReturn { get; set; } = new(
            Guid.NewGuid(),
            true,
            new ContractRatingDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Client Name",
                "Lawyer Name",
                RaterRole.Client,
                5,
                "Excellent",
                DateTime.UtcNow),
            null);

        public PagedResult<ContractRatingDto> PagedRatingsToReturn { get; set; } = new(
            [
                new ContractRatingDto(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "Client Name",
                    "Lawyer Name",
                    RaterRole.Client,
                    5,
                    "Excellent",
                    DateTime.UtcNow)
            ],
            1,
            10,
            1,
            false);


        public Task<ContractRatingDto> SubmitAsync(
            Guid contractId,
            SubmitRatingRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(RatingToReturn);
        }

        public Task<ContractRatingDto> UpdateAsync(
            Guid contractId,
            UpdateRatingRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(RatingToReturn);
        }

        public Task<ContractRatingSummaryDto> GetByContractAsync(
            Guid contractId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(SummaryToReturn);
        }

        public Task<PagedResult<ContractRatingDto>> GetByLawyerAsync(
            Guid lawyerUserId,
            LawyerRatingsQuery query,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(PagedRatingsToReturn);
        }
    }

    [Fact]
    public async Task SubmitAsync_ReturnsCreatedApiResponse()
    {
        var stubService = new StubRatingService();
        var controller = new RatingsController(stubService);
        var contractId = Guid.NewGuid();
        var request = new SubmitRatingRequest(5, "Excellent");

        var actionResult = await controller.SubmitAsync(contractId, request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);

        var response = Assert.IsType<ApiResponse<ContractRatingDto>>(objectResult.Value);
        Assert.True(response.Success);
        Assert.Equal(StatusCodes.Status201Created, response.StatusCode);
        Assert.Equal(stubService.RatingToReturn, response.Data);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsOkApiResponse()
    {
        var stubService = new StubRatingService();
        var controller = new RatingsController(stubService);
        var contractId = Guid.NewGuid();
        var request = new UpdateRatingRequest(4, "Updated comment");

        var actionResult = await controller.UpdateAsync(contractId, request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var response = Assert.IsType<ApiResponse<ContractRatingDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        Assert.Equal(stubService.RatingToReturn, response.Data);
    }


    [Fact]
    public async Task GetByContractAsync_ReturnsOkApiResponse()
    {
        var stubService = new StubRatingService();
        var controller = new RatingsController(stubService);
        var contractId = Guid.NewGuid();

        var actionResult = await controller.GetByContractAsync(contractId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var response = Assert.IsType<ApiResponse<ContractRatingSummaryDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        Assert.Equal(stubService.SummaryToReturn, response.Data);
    }

    [Fact]
    public async Task GetByLawyerAsync_ReturnsOkApiResponse()
    {
        var stubService = new StubRatingService();
        var controller = new RatingsController(stubService);
        var lawyerUserId = Guid.NewGuid();
        var query = new LawyerRatingsQuery(1, 10);

        var actionResult = await controller.GetByLawyerAsync(lawyerUserId, query, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var response = Assert.IsType<ApiResponse<PagedResult<ContractRatingDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        Assert.Equal(stubService.PagedRatingsToReturn, response.Data);
    }
}
