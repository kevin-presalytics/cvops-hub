using System;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using System.Collections.Generic;
using lib.models;
using lib.extensions;

namespace db_migrations
{

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Loading Configuration...");

            var serviceProvider = Services.ConfigureServices();

            using (var context = serviceProvider.GetRequiredService<MigrationsDbContext>())
            {
                // Console.WriteLine("Checking Database Connection...");
                // if (context.Database.CanConnect())
                // {
                //     Console.WriteLine("Database Connection is OK");
                // }
                // else
                // {
                //     Console.WriteLine("Database Connection is Unavailable");
                //     return;
                // }

                Console.WriteLine("Checking for Database Existence...");
                //context.Database.EnsureCreated();
                
                // var migrationsAssembly = context.Database.GetRelationalService<IMigrationsAssembly>();

                // Console.WriteLine("Checking for Database Migrations...");
                // var differ = context.Database.GetRelationalService<IMigrationsModelDiffer>();

                var pendingMigrations = context.Database.GetPendingMigrations();
            
                // bool hasDifferences = differ.HasDifferences(migrationsAssembly.ModelSnapshot?.Model as IRelationalModel, context.Model as IRelationalModel);

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


