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
    
    
    
    
    using Kolokwium.DTOs;
using Kolokwium.Exceptions;
using Kolokwium.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kolokwium.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly IDbService _dbService;
        public CustomersController(IDbService dbService)
        {
            _dbService = dbService;
        }

        // 1. GET - Fetching Data
        [HttpGet("{id}/rentals")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _dbService.GetCustomerRentalsAsync(id);
                return Ok(result);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        // 2. POST - Creating Data (with collection)
        [HttpPost("{id}/rentals")]
        public async Task<IActionResult> Post([FromRoute] int id, [FromBody] CreateRentalWithMoviesDto dto)
        {
            if (dto.Movies == null || !dto.Movies.Any())
                return BadRequest("At least one movie is required.");

            try
            {
                // Service will loop through dto.Movies to insert each one
                await _dbService.CreateRentalWithMoviesAsync(id, dto);
                return Created($"api/customers/{id}/rentals", dto);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        // 3. PUT - Updating Data (e.g., Updating status for a list of rentals)
        [HttpPut("{id}/rentals/status")]
        public async Task<IActionResult> UpdateRentalStatuses([FromRoute] int id, [FromBody] List<int> rentalIds)
        {
            if (!rentalIds.Any())
                return BadRequest("List of IDs cannot be empty.");

            try
            {
                // Service will use a foreach loop to update each rental ID provided
                await _dbService.UpdateRentalsStatusAsync(id, rentalIds);
                return NoContent(); 
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        // 4. DELETE - Removing Data
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                // Service might loop to delete related rentals before deleting the customer
                await _dbService.DeleteCustomerAndDataAsync(id);
                return Ok($"Customer {id} and all related records deleted.");
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Could not delete customer due to existing constraints.");
            }
        }
    }
}
    
}