using System;

namespace BookingSystem.Models
{
    public class Booking
    {
        public int BookingID { get; set; }
        public string Resource { get; set; }
        public DateTime Date { get; set; }
        public int CustomerID { get; set; }

        public Booking(int bookingId, string resource, DateTime date, int customerId)
        {
            BookingID = bookingId;
            Resource = resource;
            Date = date;
            CustomerID = customerId;
        }

        public void DisplayBookingInfo()
        {
            Console.WriteLine($"Booking ID: {BookingID}, Resource: {Resource}, Dato: {Date.ToShortDateString()}, {CustomerID}");
        }
    }
}
