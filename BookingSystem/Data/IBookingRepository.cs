using System;
using System.Collections.Generic;
using System.IO;
using BookingSystem.Models;

namespace BookingSystem.Data
{
    public interface IBookingRepository
    {
        void AddBooking(Booking booking);
        IEnumerable<Booking> GetAllBookings();
        Booking GetBookingById(int bookingId);
        void UpdateBooking(Booking booking);
        void DeleteBooking(int bookingId);
        IEnumerable<Booking> SearchBookings(string searchTerm);
        void GenerateReport(string filePath);

        int CreateCustomer(string firstName, string lastName, string email);

        bool DoesCustomerExist(int  customerId);
        IEnumerable<Customer> GetAllCustomers();
        void GeneratePdfReport(string filePath);

    }
}
