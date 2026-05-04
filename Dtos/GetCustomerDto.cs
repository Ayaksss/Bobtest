namespace WebApplication3.Dtos;

public class GetCustomerDto
{
    public string FirstName { set; get; }
    public string LastName { set; get; }
    public List<GetRentalDto> Rentals { set; get; }
}