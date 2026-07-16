using SmartCourt.Features.Users.Clients.DTOs;

namespace SmartCourt.Features.Users.Clients;

public class ClientService : IClientService
{
    public Task<ClientProfileResponse> GetProfileAsync(Guid id)
    {
        /*
         * ALGORITHM:
         * 1. Query the database for the user with the specified 'id' where Role is 'Client'.
         * 2. If user does not exist, throw NotFoundException.
         * 3. Map the database entity to ClientProfileResponse.
         * 4. Explicitly exclude any file navigation properties.
         * 5. Return the mapped DTO.
         */
        throw new NotImplementedException();
    }

    public Task UpdateProfileAsync(Guid id, UpdateClientProfileRequest request)
    {
        /*
         * ALGORITHM:
         * 1. Retrieve the existing Client entity by 'id'.
         * 2. If not found, throw NotFoundException.
         * 3. Update only the permitted fields on the entity (Email, PhoneNumber, DateOfBirth, Address).
         * 4. Save changes to the database.
         */
        throw new NotImplementedException();
    }

    public Task DeleteProfileAsync(Guid id)
    {
        /*
         * ALGORITHM:
         * 1. Retrieve the existing Client entity by 'id'.
         * 2. If not found, throw NotFoundException.
         * 3. Perform a soft-delete (or hard delete depending on policy) of the user and their profile.
         * 4. Save changes to the database.
         */
        throw new NotImplementedException();
    }
}
