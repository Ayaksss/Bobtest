using Microsoft.Data.SqlClient;

namespace WebApplication3.Services;

using WebApplication3.Dtos;

public class DbService : IDbService
{
     
    private readonly string _connectionString;

    public DbService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;
    }
    
    
    public async Task<GetCustomerDto> GetCustomerRentals(int id)
    {
        var query = """

                    SELECT c.first_name as FirstName, 
                           c.last_name as LastName,
                           r.rental_id as RentalId,
                           r.rental_date as RentalDate,
                           r.return_date as ReturnDate,
                           s.name as Status,
                           m.title as MovieTitle,
                           ri.price_at_rental as PriceAtRental
                    FROM Customer c JOIN Rental r on c.customer_id = r.customer_id
                    JOIN Status s on r.status_id = s.status_id
                    JOIN Rental_Item ri on r.rental_id = ri.rental_id
                    JOIN Movie m on ri.movie_id = m.movie_id
                    WHERE c.customer_id = @customerId;
                    """;
        
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new SqlCommand(query, connection);
        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@customerId", id);
        
        await using var reader = await command.ExecuteReaderAsync();
        
        GetCustomerDto? result = null;
        
        var ordFirstName = reader.GetOrdinal("FirstName");
        var ordLastName = reader.GetOrdinal("LastName");
        var ordRentalId = reader.GetOrdinal("RentalId"); // Use ID, not Date!
        var ordRentalDate = reader.GetOrdinal("RentalDate");
        var ordReturnDate = reader.GetOrdinal("ReturnDate");
        var ordStatus = reader.GetOrdinal("Status");
        var ordMovieTitle = reader.GetOrdinal("MovieTitle");
        var ordPriceAtRental = reader.GetOrdinal("PriceAtRental");

        while (await reader.ReadAsync())
        {

            if (result == null)
            {
                result = new GetCustomerDto()
                {
                    FirstName = reader.GetString(ordFirstName),
                    LastName = reader.GetString(ordLastName),
                    Rentals = new List<GetRentalDto>()
                };
            }

            var rentalId = reader.GetInt32(ordRentalId);

            var rental = result.Rentals.FirstOrDefault(x => x.IdRental == rentalId);

            if (rental == null)
            {
                rental = new GetRentalDto()
                {
                    DateRental = reader.GetDateTime(ordRentalDate),
                    DateReturn = reader.IsDBNull(ordReturnDate) ? null : reader.GetDateTime(ordReturnDate),
                    Status = reader.GetString(ordStatus),
                    IdRental = rentalId,
                    Movies = new List<GetMovieDetaildDto>()
                };

                result.Rentals.Add(rental);
            }

            rental.Movies.Add(new GetMovieDetaildDto()
            {
                Title = reader.GetString(ordMovieTitle),
                PriceAtRental = reader.GetDecimal(ordPriceAtRental),
            });
        }

        return result ?? throw new NullReferenceException();
        
        throw new NotImplementedException();
    }

    public async Task CreateRentalWithMovies(int customerId ,CreateRentalDto dto)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var transaction = await connection.BeginTransactionAsync();
        await using var command = new SqlCommand();
        command.Connection = connection;
        command.Transaction = transaction as SqlTransaction;
        
        var createRentalQuery = """
                                INSERT INTO Rental
                                VALUES(@RentalDate, @ReturnDate, @CustomerId, @StatusId)
                                SELECT @@IDENTITY;
                                """;

        var createRentalItemQuery = """
                                    INSERT INTO Rental_Item
                                    VALUES(@RentalId, @MovieId, @Price);
                                    """;

        var getMovieIdQuery = """
                              SELECT movie_id
                              FROM Movie
                              WHERE title = @MovieTitle;
                              """;

        var checkCustomerQuery = """
                                 SELECT 1 
                                 FROM Customer 
                                 WHERE customer_id = @IdCustomer;
                                 """;
        
        try
        {
            command.Parameters.Clear();
            command.CommandText = checkCustomerQuery;
            command.Parameters.AddWithValue("@IdCustomer", customerId);
            var cutomerIdRes =  await command.ExecuteScalarAsync();
            if (cutomerIdRes == null)
            {
                
            }
            
            command.Parameters.Clear();
            command.CommandText = createRentalQuery;
            command.Parameters.AddWithValue("@CustomerId", customerId);
            command.Parameters.AddWithValue("@RentalDate", dto.StartDate);
            command.Parameters.AddWithValue("@ReturnDate", DBNull.Value);
            command.Parameters.AddWithValue("@StatusId", 1);
                
            
            
            var rentalIdRes = await command.ExecuteScalarAsync();
            
            var rentalId = Convert.ToInt32(rentalIdRes);
            
            if (rentalIdRes == null)
            {
                
            }

            foreach (var movie in dto.Movies)
            {
                command.Parameters.Clear();
                command.CommandText = getMovieIdQuery;
                command.Parameters.AddWithValue("@MovieTitle", movie.Title);
                
                var movieIdRes = await command.ExecuteScalarAsync();

                if (movieIdRes == null) ;
                
                command.Parameters.Clear();
                command.CommandText = createRentalItemQuery;
                command.Parameters.AddWithValue("@RentalId", rentalId);
                command.Parameters.AddWithValue("@MovieId", movieIdRes);
                command.Parameters.AddWithValue("@Price", movie.PriceAtRent);
                
                await command.ExecuteNonQueryAsync();

            }
            
            await transaction.CommitAsync();
            
            
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            Console.WriteLine(e);
            throw;
        }

        
    }

    public async Task<bool> UpdateRental(int rentalId, DateTime returnDate)
    {
        var query = "UPDATE Rental SET return_date = @ReturnDate WHERE rental_id = @RentalId";
        
        
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand();
        command.CommandText = query;
        command.Connection = connection;
        
        command.Parameters.Clear();
        command.Parameters.AddWithValue("@ReturnDate", returnDate);
        command.Parameters.AddWithValue("@RentalId", rentalId);
        
        int rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
    
    public async Task<bool> DeleteRental(int rentalId)
    {
        var query = "DELETE FROM Rental WHERE rental_id = @RentalId";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@RentalId", rentalId);

        return await command.ExecuteNonQueryAsync() > 0;
    }
    
    public async Task<bool> DeleteRentalWithItems(int rentalId)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();
        await using var command = new SqlCommand { Connection = connection, Transaction = transaction as SqlTransaction };

        try
        {
            // Step 1: Delete children (Rental_Items)
            command.CommandText = "DELETE FROM Rental_Item WHERE rental_id = @Id";
            command.Parameters.AddWithValue("@Id", rentalId);
            await command.ExecuteNonQueryAsync();

            // Step 2: Delete parent (Rental)
            command.CommandText = "DELETE FROM Rental WHERE rental_id = @Id";
            // Parameter @Id is already added, so we just run it
            int rowsAffected = await command.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
            return rowsAffected > 0;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}