using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using Microsoft.Data.SqlClient;
using BookingSystem.Models;

namespace BookingSystem.Data
{
    public class BookingRepository : IBookingRepository
    {
        private readonly string _connectionString;

        public BookingRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
                
        public void GeneratePdfReport(string filePath)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var bookings = GetAllBookings();

            if (bookings == null || !bookings.Any())
            {
                Console.WriteLine("Ingen bookinger fundet. Rapporten er ikke genereret.");
                return;
            }
                        
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);

                    page.Header().Text("Booking Rapport").FontSize(20).Bold().FontColor(Colors.Blue.Medium);

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(60);  // BookingID
                            columns.RelativeColumn(2);  // Resource
                            columns.RelativeColumn(2);  // Date
                            columns.ConstantColumn(100); // CustomerID
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("BookingID").Bold();
                            header.Cell().Text("Ressource").Bold();
                            header.Cell().Text("Dato").Bold();
                            header.Cell().Text("Kunde ID").Bold();
                        });

                        foreach (var booking in bookings)
                        {
                            table.Cell().Text(booking.BookingID.ToString());
                            table.Cell().Text(booking.Resource);
                            table.Cell().Text(booking.Date.ToShortDateString());
                            table.Cell().Text(booking.CustomerID.ToString());
                        }
                    });

                    page.Footer().Text(x =>
                    {
                        x.Span("Rapport genereret den ").Bold();
                        x.Span(DateTime.Now.ToString("dd-MM-yyyy HH:mm"));
                    });
                });
            }).GeneratePdf(filePath);

            Console.WriteLine($"PDF-rapport genereret: {filePath}");
        }
        public void AddBooking(Booking booking)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = "INSERT INTO Bookings (BookingID, Resource, Date, CustomerID) VALUES (@BookingID, @Resource, @Date,@CustomerID)";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@BookingID", booking.BookingID);
            command.Parameters.AddWithValue("@Resource", booking.Resource);
            command.Parameters.AddWithValue("@Date", booking.Date);
            command.Parameters.AddWithValue("@CustomerID", booking.CustomerID);

            command.ExecuteNonQuery();
        }

        public IEnumerable<Booking> GetAllBookings()
        {
            var bookings = new List<Booking>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = "SELECT BookingID, Resource, Date, CustomerID FROM Bookings";
            using var command = new SqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                bookings.Add(new Booking(
                    reader.GetInt32(0),     // BookingID
                    reader.GetString(1),    // Resource
                    reader.GetDateTime(2),  // Date
                    reader.GetInt32(3)      // CustomerID (Tilføjet her)
                ));
            }

            return bookings;
        }


        public Booking GetBookingById(int bookingId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = "SELECT BookingID, Resource, Date, CustomerID FROM Bookings WHERE BookingID = @BookingID";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@BookingID", bookingId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Booking(
                    reader.GetInt32(0),    // BookingID
                    reader.GetString(1),   // Resource
                    reader.GetDateTime(2), // Date
                    reader.GetInt32(3)     // CustomerID (Tilføjet her)
                );
            }

            return null;
        }


        public void UpdateBooking(Booking booking)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = "UPDATE Bookings SET Resource = @Resource, Date = @Date WHERE BookingID = @BookingID";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@BookingID", booking.BookingID);
            command.Parameters.AddWithValue("@Resource", booking.Resource);
            command.Parameters.AddWithValue("@Date", booking.Date);

            command.ExecuteNonQuery();
        }

        public void DeleteBooking(int bookingId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = "DELETE FROM Bookings WHERE BookingID = @BookingID";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@BookingID", bookingId);

            command.ExecuteNonQuery();
        }

        public IEnumerable<Booking> SearchBookings(string searchTerm)
        {
            var bookings = new List<Booking>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
        SELECT BookingID, Resource, Date, CustomerID
        FROM Bookings
        WHERE 
            (Resource IS NOT NULL AND Resource LIKE @SearchTerm)
            OR CAST(BookingID AS NVARCHAR) LIKE @SearchTerm
            OR CAST(CustomerID AS NVARCHAR) LIKE @SearchTerm
    ";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                bookings.Add(new Booking(
                    reader.GetInt32(0),    // BookingID
                    reader.GetString(1),   // Resource
                    reader.GetDateTime(2), // Date
                    reader.GetInt32(3)     // CustomerID
                ));
            }

            return bookings;
        }



        public void GenerateReport(string filePath)
        {
            var bookings = GetAllBookings();

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("BookingID, Resource, Date");
                foreach (var booking in bookings)
                {
                    writer.WriteLine($"{booking.BookingID}, {booking.Resource}, {booking.Date.ToShortDateString()}");
                }
            }

            Console.WriteLine($"Rapport genereret: {filePath}");
        }
        public int CreateCustomer(string firstName, string lastName, string email)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = "INSERT INTO Customers (FirstName, LastName, Email) OUTPUT INSERTED.CustomerID VALUES (@FirstName, @LastName, @Email)";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@FirstName", firstName);
            command.Parameters.AddWithValue("@LastName", lastName);
            command.Parameters.AddWithValue("@Email", email);

            return (int)command.ExecuteScalar(); // Returnerer det nye CustomerID
        }
        public bool DoesCustomerExist(int customerId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = "SELECT COUNT(1) FROM Customers WHERE CustomerID = @CustomerID";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerID", customerId);

            return (int)command.ExecuteScalar() > 0; // Returnerer true hvis kunden findes
        }
        public IEnumerable<Customer> GetAllCustomers()
        {
            var customers = new List<Customer>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = "SELECT CustomerID, FirstName, LastName, Email FROM Customers";
            using var command = new SqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                customers.Add(new Customer(
                    reader.GetString(1), // FirstName
                    reader.GetString(2), // LastName
                    reader.GetString(3), // Email
                    reader.GetInt32(0)   // CustomerID
                ));
            }

            return customers;
        }

    }
}
