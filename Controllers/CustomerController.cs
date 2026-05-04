using Microsoft.AspNetCore.Mvc;
using WebApplication3.Services;
using WebApplication3.Dtos;
using WebApplication3.Services;


namespace WebApplication3.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomerController : ControllerBase
{
    private readonly IDbService _dbService;
    public CustomerController(IDbService dbService)
    {
        _dbService = dbService;
    }
        
    [Route("{id}/rentals")]
    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await _dbService.GetCustomerRentals(id);
            return Ok(result);
        }
        catch (Exception e)
        {
            return NotFound(e.Message);
        }
    }
    
    [Route("{id}/rentals")]
    [HttpPost]
    public async Task<IActionResult> Post([FromRoute] int id, [FromBody] CreateRentalDto dto)
    {
        if (!dto.Movies.Any())
        {
            return BadRequest("At least one item is required.");
        }
            
        try
        {
            await _dbService.CreateRentalWithMovies(id, dto);
            return Created($"api/customers/{id}/rentals", dto);
        }
        catch (Exception e)
        {
            return NotFound(e.Message);
        }
            
    }
    
    [HttpPut("rentals/{rentalId}")]
    public async Task<IActionResult> UpdateRental(int rentalId, [FromBody] DateTime returnDate)
    {
        // The service returns true if a row was updated, false if ID wasn't found
        bool success = await _dbService.UpdateRental(rentalId, returnDate);

        if (!success)
        {
            return NotFound($"Rental with ID {rentalId} not found.");
        }

        return NoContent(); // Success (204)
    }
    
}