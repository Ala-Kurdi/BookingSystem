using System;

namespace BookingSystem.Models
{
    public class Customer : User
    {
        public int CustomerID { get; set; }

        public Customer(string firstName, string lastName, string email, int customerId)
            : base(firstName, lastName, email)
        {
            CustomerID = customerId;
        }

        public override void DisplayInfo()
        {
            Console.WriteLine($"Kunde: {FirstName} {LastName}, Email: {Email}, ID: {CustomerID}");
        }
    }
}
