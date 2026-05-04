using WebApplication3.Dtos;

namespace WebApplication3.Services;

public interface IDbService
{
    Task<GetCustomerDto> GetCustomerRentals(int id);
    
    Task CreateRentalWithMovies(int customerId, CreateRentalDto dto);

    Task<bool> UpdateRental(int rentalId, DateTime startDate);
}