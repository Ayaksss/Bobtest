namespace WebApplication3.Dtos;

public class CreateRentalDto
{
    public DateTime StartDate { get; set; }
    public List<CreateMovieDto> Movies { get; set; }
}