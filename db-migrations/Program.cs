﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using lib.models.configuration;
using lib.extensions;
using lib.services;
using lib.models;

namespace db_migrations
{

    public class Program
    {
        public static void Main(string[] args)
        {
            DebuggerSetup.WaitForDebugger();
            Console.WriteLine("Loading Configuration...");

            var serviceProvider = Services.ConfigureServices();
            var appConfiguration = serviceProvider.GetRequiredService<AppConfiguration>();
            var contextFactory = serviceProvider.GetRequiredService<IDbContextFactory<CvopsDbContext>>();

            using (var context = contextFactory.CreateDbContext())
            {
                
                Console.WriteLine("Checking for Database Existence...");

                var pendingMigrations = context.Database.GetPendingMigrations();

                if (appConfiguration.Logging.Level.ToString() == "Debug") {
                    Console.WriteLine("Using Connection string: {0}", appConfiguration.GetPostgresqlConnectionString());
                }
            
                if (pendingMigrations.Any()) {
                    Console.WriteLine($"{pendingMigrations.Count()} Database Migrations are required");
                    var currentMigration = context.Database.GetAppliedMigrations().LastOrDefault();
                    Console.WriteLine($"Current Migration: {currentMigration}");
                    Console.WriteLine($"Latest Migration: {pendingMigrations.Last()}");
                    Console.WriteLine("Applying Database Migrations...");
                    context.Database.Migrate();
                    Console.WriteLine("Database Migrations Applied");
                } else {
                    Console.WriteLine("Database Migrations are not Required");
                } 
            }
        }
    }
}


