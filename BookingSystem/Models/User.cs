using System;

namespace BookingSystem.Models
{
    public abstract class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public User(string firstName, string lastName, string email)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }

        public abstract void DisplayInfo();
    }
}
