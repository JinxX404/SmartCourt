using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Users.Clients;
using SmartCourt.Features.Users.Clients.DTOs;
using SmartCourt.Features.Users.Clients.Validators;
using SmartCourt.Features.Users.Lawyers;
using SmartCourt.Features.Users.Lawyers.DTOs;
using SmartCourt.Features.Users.Lawyers.Validators;
using SmartCourt.Tests.Features.Auth;
using Xunit;

namespace SmartCourt.Tests.Features.Users;

public sealed class CompleteProfileAgeValidationTests
{
    [Fact]
    public async Task ClientService_CompleteProfile_Under21_ThrowsBusinessException()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var client = await testContext.CreateUserAsync(UserStatus.PendingVerification, emailConfirmed: true);
        var service = new ClientService(
            testContext.UserManager,
            testContext.DbContext,
            new TestCurrentUserService(client.Id),
            new TestAuthHelperService(),
            new TestFileStorageService());

        var today = DateOnly.FromDateTime(DateTime.Today);
        var under21Dob = today.AddYears(-20);

        var request = new CompleteClientProfileRequest
        {
            PhoneNumber = "+201234567890",
            NationalNumber = "12345678901234",
            Gender = Gender.Male,
            DateOfBirth = under21Dob,
            Address = "Test Address",
            Governorate = "Cairo",
            City = "Cairo"
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            service.CompleteProfileAsync(request, CancellationToken.None));

        Assert.Equal("يجب أن يكون عمر المستخدم 21 عاماً أو أكثر.", exception.Message);
    }

    [Fact]
    public async Task ClientService_CompleteProfile_21OrOlder_Succeeds()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var client = await testContext.CreateUserAsync(UserStatus.PendingVerification, emailConfirmed: true);
        var service = new ClientService(
            testContext.UserManager,
            testContext.DbContext,
            new TestCurrentUserService(client.Id),
            new TestAuthHelperService(),
            new TestFileStorageService());

        var today = DateOnly.FromDateTime(DateTime.Today);
        var age21Dob = today.AddYears(-21);

        var request = new CompleteClientProfileRequest
        {
            PhoneNumber = "+201234567890",
            NationalNumber = "12345678901234",
            Gender = Gender.Male,
            DateOfBirth = age21Dob,
            Address = "Test Address",
            Governorate = "Cairo",
            City = "Cairo"
        };

        await service.CompleteProfileAsync(request, CancellationToken.None);

        var updatedUser = await testContext.ReloadUserAsync(client.Id);
        Assert.Equal(UserStatus.PendingReview, updatedUser.Status);
        Assert.Equal(age21Dob, updatedUser.DateOfBirth);
    }

    [Fact]
    public async Task ClientService_UpdateProfile_Under21_ThrowsBusinessException()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var client = await testContext.CreateUserAsync(UserStatus.Active, emailConfirmed: true);
        var service = new ClientService(
            testContext.UserManager,
            testContext.DbContext,
            new TestCurrentUserService(client.Id),
            new TestAuthHelperService(),
            new TestFileStorageService());

        var today = DateOnly.FromDateTime(DateTime.Today);
        var under21Dob = today.AddYears(-20);

        var request = new UpdateClientProfileRequest
        {
            NationalNumber = "12345678901234",
            DateOfBirth = under21Dob
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            service.UpdateProfileAsync(request, CancellationToken.None));

        Assert.Equal("يجب أن يكون عمر المستخدم 21 عاماً أو أكثر.", exception.Message);
    }

    [Fact]
    public async Task LawyerService_CompleteProfile_Under21_ThrowsBusinessException()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var lawyer = await testContext.CreateUserAsync(UserStatus.PendingVerification, emailConfirmed: true);
        var service = new LawyerService(
            testContext.UserManager,
            testContext.DbContext,
            new TestCurrentUserService(lawyer.Id),
            new TestAuthHelperService(),
            new TestFileStorageService());

        var today = DateOnly.FromDateTime(DateTime.Today);
        var under21Dob = today.AddYears(-20);

        var request = new CompleteLawyerProfileRequest
        {
            PhoneNumber = "+201234567890",
            NationalNumber = "12345678901234",
            Gender = Gender.Male,
            DateOfBirth = under21Dob,
            Address = "Test Address",
            Governorate = "Cairo",
            City = "Cairo",
            Level = LawyerLevel.GeneralRegistration,
            Specializations = new List<LawyerSpecializationDto>
            {
                new LawyerSpecializationDto
                {
                    Specialization = LawyerSpecializationType.CivilLaw,
                    YearsOfExperience = 2,
                    CasesHandled = 5
                }
            }
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            service.CompleteProfileAsync(request, CancellationToken.None));

        Assert.Equal("يجب أن يكون عمر المستخدم 21 عاماً أو أكثر.", exception.Message);
    }

    [Fact]
    public async Task LawyerService_CompleteProfile_21OrOlder_Succeeds()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var lawyer = await testContext.CreateUserAsync(UserStatus.PendingVerification, emailConfirmed: true);
        var service = new LawyerService(
            testContext.UserManager,
            testContext.DbContext,
            new TestCurrentUserService(lawyer.Id),
            new TestAuthHelperService(),
            new TestFileStorageService());

        var today = DateOnly.FromDateTime(DateTime.Today);
        var age21Dob = today.AddYears(-21);

        var request = new CompleteLawyerProfileRequest
        {
            PhoneNumber = "+201234567890",
            NationalNumber = "12345678901234",
            Gender = Gender.Male,
            DateOfBirth = age21Dob,
            Address = "Test Address",
            Governorate = "Cairo",
            City = "Cairo",
            Level = LawyerLevel.GeneralRegistration,
            Specializations = new List<LawyerSpecializationDto>
            {
                new LawyerSpecializationDto
                {
                    Specialization = LawyerSpecializationType.CivilLaw,
                    YearsOfExperience = 2,
                    CasesHandled = 5
                }
            }
        };

        await service.CompleteProfileAsync(request, CancellationToken.None);

        var updatedUser = await testContext.ReloadUserAsync(lawyer.Id);
        Assert.Equal(UserStatus.PendingReview, updatedUser.Status);
        Assert.Equal(age21Dob, updatedUser.DateOfBirth);
    }

    [Fact]
    public async Task LawyerService_UpdateProfile_Under21_ThrowsBusinessException()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var lawyer = await testContext.CreateUserAsync(UserStatus.Active, emailConfirmed: true);
        var service = new LawyerService(
            testContext.UserManager,
            testContext.DbContext,
            new TestCurrentUserService(lawyer.Id),
            new TestAuthHelperService(),
            new TestFileStorageService());

        var today = DateOnly.FromDateTime(DateTime.Today);
        var under21Dob = today.AddYears(-20);

        var request = new UpdateLawyerProfileRequest
        {
            NationalNumber = "12345678901234",
            DateOfBirth = under21Dob,
            Level = LawyerLevel.GeneralRegistration
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            service.UpdateProfileAsync(request, CancellationToken.None));

        Assert.Equal("يجب أن يكون عمر المستخدم 21 عاماً أو أكثر.", exception.Message);
    }

    [Fact]
    public void CompleteClientProfileRequestValidator_Under21_FailsValidation()
    {
        var validator = new CompleteClientProfileRequestValidator();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var under21Dob = today.AddYears(-20);

        var request = new CompleteClientProfileRequest
        {
            PhoneNumber = "+201234567890",
            NationalNumber = "12345678901234",
            Gender = Gender.Male,
            DateOfBirth = under21Dob
        };

        var result = validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.DateOfBirth));
    }

    [Fact]
    public void UpdateClientProfileRequestValidator_Under21_FailsValidation()
    {
        var validator = new UpdateClientProfileRequestValidator();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var under21Dob = today.AddYears(-20);

        var request = new UpdateClientProfileRequest
        {
            NationalNumber = "12345678901234",
            DateOfBirth = under21Dob
        };

        var result = validator.Validate(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CompleteLawyerProfileRequestValidator_Under21_FailsValidation()
    {
        var validator = new CompleteLawyerProfileRequestValidator();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var under21Dob = today.AddYears(-20);

        var request = new CompleteLawyerProfileRequest
        {
            PhoneNumber = "+201234567890",
            NationalNumber = "12345678901234",
            Gender = Gender.Male,
            DateOfBirth = under21Dob,
            Level = LawyerLevel.GeneralRegistration,
            Specializations = new List<LawyerSpecializationDto>
            {
                new LawyerSpecializationDto
                {
                    Specialization = LawyerSpecializationType.CivilLaw,
                    YearsOfExperience = 1,
                    CasesHandled = 1
                }
            }
        };

        var result = validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.DateOfBirth));
    }

    [Fact]
    public void UpdateLawyerProfileRequestValidator_Under21_FailsValidation()
    {
        var validator = new UpdateLawyerProfileRequestValidator();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var under21Dob = today.AddYears(-20);

        var request = new UpdateLawyerProfileRequest
        {
            NationalNumber = "12345678901234",
            DateOfBirth = under21Dob,
            Level = LawyerLevel.GeneralRegistration
        };

        var result = validator.Validate(request);
        Assert.False(result.IsValid);
    }
}
