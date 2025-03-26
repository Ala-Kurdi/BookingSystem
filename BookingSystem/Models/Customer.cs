using System;

namespace BookingSystem.Models
{
    // Arver fra User
    public class Customer : User
    {
        public int CustomerID { get; set; }

        public Customer(string firstName, string lastName, string email, int customerId)
            : base(firstName, lastName, email)
        {
            CustomerID = customerId;
        }

        // Implementerer DisplayInfo() fra User
        public override void DisplayInfo()
        {
            Console.WriteLine($"Kunde: {FirstName} {LastName}, Email: {Email}, ID: {CustomerID}");
        }
    }
}
