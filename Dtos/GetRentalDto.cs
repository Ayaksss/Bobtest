namespace WebApplication3.Dtos;

public class GetRentalDto
{
    public int IdRental { set; get; }
    public DateTime DateRental { set; get; }
    public DateTime? DateReturn { set; get; }
    public string Status { set; get; }
    public List<GetMovieDetaildDto> Movies { set; get; }
}