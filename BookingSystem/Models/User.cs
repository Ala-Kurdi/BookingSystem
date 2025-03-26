using System;

namespace BookingSystem.Models
{
    // Abstrakt klasse User
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

        // Abstrakt metode som skal implementeres af nedarvede klasser
        public abstract void DisplayInfo();
    }
}
