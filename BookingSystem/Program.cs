﻿using System;
using System.Runtime.InteropServices;
using BookingSystem.Data;
using BookingSystem.Models;
using BookingSystem.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuestPDF.Infrastructure;

namespace BookingSystem
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_RESTORE = 9;

        static void Main(string[] args)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            if (args.Length > 0 && args[0].Equals("web", StringComparison.OrdinalIgnoreCase))
            {
                RunWebApp();
            }
            else
            {
                IntPtr consoleWindow = GetConsoleWindow();
                ShowWindow(consoleWindow, SW_RESTORE);
                RunConsoleApp();
            }
        }

        static void RunConsoleApp()
        {
            var connectionString = "Server=ADEMAKAT\\SQLEXPRESS;Database=BookingDB;Trusted_Connection=True;Encrypt=False;";
            IBookingRepository bookingRepository = new BookingRepository(connectionString);
            EmailService emailService = new EmailService();

            while (true)
            {
                Console.WriteLine("=== Booking System ===");
                Console.WriteLine("1. Opret booking");
                Console.WriteLine("2. Vis alle bookinger");
                Console.WriteLine("3. Søg efter booking");
                Console.WriteLine("4. Opdater booking");
                Console.WriteLine("5. Slet booking");
                Console.WriteLine("6. Generér rapport");
                Console.WriteLine("7. Send notifikation");
                Console.WriteLine("8. Vis alle kunder");
                Console.WriteLine("9. Generate PDF Rapport");
                Console.WriteLine("0. Afslut");
                Console.Write("\nVælg en mulighed: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        CreateBooking(bookingRepository);
                        break;
                    case "2":
                        ShowAllBookings(bookingRepository);
                        break;
                    case "3":
                        SearchBooking(bookingRepository);
                        break;
                    case "4":
                        UpdateBooking(bookingRepository);
                        break;
                    case "5":
                        DeleteBooking(bookingRepository);
                        break;
                    case "6":
                        GenerateReport(bookingRepository);
                        break;
                    case "7":
                        SendNotification(emailService);
                        break;
                    case "8":
                        ShowAllCustomers(bookingRepository);
                        break;
                    case "9":
                        GeneratePdfReport(bookingRepository);
                        break;
                    case "0":
                        Console.WriteLine("Program afsluttes...");
                        return;
                    default:
                        Console.WriteLine("Ugyldigt valg! Prøv igen.");
                        break;
                }

                Console.WriteLine("\nTryk på en vilkårlig tast for at fortsætte...");
            }
        }

        static void RunWebApp()
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls("http://localhost:5000");

            var connectionString = "Server=ADEMAKAT\\SQLEXPRESS;Database=BookingDB;Trusted_Connection=True;Encrypt=False;";
            builder.Services.AddSingleton<IBookingRepository>(new BookingRepository(connectionString));
            builder.Services.AddSingleton<EmailService>();

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            app.UseRouting();
            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", () => "Velkommen til Booking System (Web Version)");

                endpoints.MapGet("/api/bookings", (IBookingRepository bookingRepository) =>
                {
                    var bookings = bookingRepository.GetAllBookings();
                    return Results.Json(bookings);
                });

                endpoints.MapGet("/api/customers", (IBookingRepository bookingRepository) =>
                {
                    var customers = bookingRepository.GetAllCustomers();
                    return Results.Json(customers);
                });

                endpoints.MapPost("/api/customers", (IBookingRepository bookingRepository, CustomerData customer) =>
                {
                    var id = bookingRepository.CreateCustomer(customer.FirstName, customer.LastName, customer.Email);
                    return Results.Ok(new { CustomerID = id });
                });

                endpoints.MapPost("/create", (IBookingRepository bookingRepository, Booking newBooking) =>
                {
                    bookingRepository.AddBooking(newBooking);
                    return Results.Ok("Booking tilføjet!");
                });

                endpoints.MapPut("/update", (IBookingRepository bookingRepository, Booking updatedBooking) =>
                {
                    bookingRepository.UpdateBooking(updatedBooking);
                    return Results.Ok("Booking opdateret succesfuldt!");
                });

                endpoints.MapDelete("/delete/{id}", (IBookingRepository bookingRepository, int id) =>
                {
                    bookingRepository.DeleteBooking(id);
                    return Results.Ok("Booking slettet succesfuldt!");
                });

                endpoints.MapPost("/send-notification", (EmailService emailService, string recipientEmail, string subject, string message) =>
                {
                    emailService.SendNotification(recipientEmail, subject, message);
                    return Results.Ok("Notifikation sendt!");
                });

                endpoints.MapGet("/report", (IBookingRepository bookingRepository) =>
                {
                    string filePath = "BookingRapport.csv";
                    bookingRepository.GenerateReport(filePath);
                    return Results.Ok($"Rapport genereret: {filePath}");
                });
            });

            app.Run();
        }

        public record CustomerData(string FirstName, string LastName, string Email);

        // --------- Booking funktioner ---------

        static void CreateBooking(IBookingRepository bookingRepository)
        {
            Console.Write("Indtast ressource: ");
            string resource = Console.ReadLine();

            Console.Write("Indtast dato (yyyy-mm-dd): ");
            DateTime date = DateTime.Parse(Console.ReadLine());

            Console.Write("Indtast CustomerID (eller tryk Enter for at tilføje ny kunde): ");
            string customerInput = Console.ReadLine();

            int customerId;
            if (string.IsNullOrWhiteSpace(customerInput))
            {
                Console.Write("Indtast kundens fornavn: ");
                string customerFirstName = Console.ReadLine();

                Console.Write("Indtast kundens efternavn: ");
                string customerLastName = Console.ReadLine();

                Console.Write("Indtast kundens email: ");
                string customerEmail = Console.ReadLine();

                customerId = bookingRepository.CreateCustomer(customerFirstName, customerLastName, customerEmail);
                Console.WriteLine($"Ny kunde oprettet med ID: {customerId}");
            }
            else
            {
                customerId = int.Parse(customerInput);
            }

            var newBooking = new Booking(new Random().Next(1000, 9999), resource, date, customerId);
            bookingRepository.AddBooking(newBooking);
            Console.WriteLine("Booking tilføjet!");
        }

        static void ShowAllBookings(IBookingRepository bookingRepository)
        {
            var bookings = bookingRepository.GetAllBookings();
            foreach (var booking in bookings)
            {
                Console.WriteLine($"ID: {booking.BookingID} - {booking.Resource} - {booking.Date.ToShortDateString()}");
            }
        }

        static void SearchBooking(IBookingRepository bookingRepository)
        {
            Console.Write("Indtast søgeord (ressource, ID eller kunde-ID): ");
            string searchTerm = Console.ReadLine();

            var searchResults = bookingRepository.SearchBookings(searchTerm).ToList();

            if (!searchResults.Any())
            {
                Console.WriteLine("Ingen bookinger matcher søgeordet. Prøv igen med et andet søgeord.");
                return;
            }

            Console.WriteLine("\n=== Søgeresultater ===");
            foreach (var booking in searchResults)
            {
                Console.WriteLine($"ID: {booking.BookingID} - {booking.Resource} - {booking.Date.ToShortDateString()} - Kunde ID: {booking.CustomerID}");
            }
        }


        static void UpdateBooking(IBookingRepository bookingRepository)
        {
            Console.Write("Indtast ID på booking der skal opdateres: ");
            if (!int.TryParse(Console.ReadLine(), out int bookingID))
            {
                Console.WriteLine("Ugyldigt BookingID! Opdatering afbrydes.");
                return;
            }

            Console.Write("Indtast ny ressource: ");
            string newResource = Console.ReadLine();

            Console.Write("Indtast ny dato (yyyy-mm-dd): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime newDate))
            {
                Console.WriteLine("Ugyldig dato! Opdatering afbrydes.");
                return;
            }

            Console.Write("Indtast CustomerID: ");
            if (!int.TryParse(Console.ReadLine(), out int customerID))
            {
                Console.WriteLine("Ugyldigt CustomerID! Opdatering afbrydes.");
                return;
            }

            var updatedBooking = new Booking(bookingID, newResource, newDate, customerID);
            bookingRepository.UpdateBooking(updatedBooking);
            Console.WriteLine("Booking opdateret succesfuldt!");
        }

        static void DeleteBooking(IBookingRepository bookingRepository)
        {
            Console.Write("Indtast ID på booking der skal slettes: ");
            int bookingID = int.Parse(Console.ReadLine());

            bookingRepository.DeleteBooking(bookingID);
            Console.WriteLine("Booking slettet succesfuldt!");
        }

        static void GenerateReport(IBookingRepository bookingRepository)
        {
            string filePath = "BookingRapport.csv";
            bookingRepository.GenerateReport(filePath);
            Console.WriteLine($"Rapport genereret: {filePath}");
        }

        static void SendNotification(EmailService emailService)
        {
            Console.Write("Indtast modtagerens e-mail: ");
            string recipientEmail = Console.ReadLine();

            Console.Write("Indtast emne for notifikation: ");
            string subject = Console.ReadLine();

            Console.Write("Indtast besked til notifikation: ");
            string message = Console.ReadLine();

            emailService.SendNotification(recipientEmail, subject, message);
        }

        static void ShowAllCustomers(IBookingRepository bookingRepository)
        {
            var customers = bookingRepository.GetAllCustomers();

            if (!customers.Any())
            {
                Console.WriteLine("Ingen kunder fundet.");
                return;
            }

            Console.WriteLine("=== Liste over kunder ===");
            foreach (var customer in customers)
            {
                Console.WriteLine($"ID: {customer.CustomerID} - {customer.FirstName} {customer.LastName} - {customer.Email}");
            }
        }

        static void GeneratePdfReport(IBookingRepository bookingRepository)
        {
            Console.Write("Indtast filnavn til rapporten (f.eks. Rapport.pdf): ");
            string fileName = Console.ReadLine();

            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

            bookingRepository.GeneratePdfReport(filePath);
        }
    }
}
