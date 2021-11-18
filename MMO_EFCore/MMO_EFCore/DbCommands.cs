using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;


namespace MMO_EFCore
{
    public class DbCommands
    {
        public static void InitializeDB(bool forceReset = false)
        {
            using (AppDbContext db = new AppDbContext())
            {
                if (!forceReset && (db.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists())
                    return;

                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                Console.WriteLine("DB Initialized");
            }
        }

        public static void CreateTestData(AppDbContext db)
        {
            var player = new Player()
            {
                Name = "yj"
            };

            List<Item> items = new List<Item>()
            {
                new Item()
                {
                    TemplateId = 101,
                    CreateDate = DateTime.Now,
                    Owner = player
                },
                new Item()
                {
                    TemplateId = 102,
                    CreateDate = DateTime.Now,
                    Owner = player
                },
                new Item()
                {
                    TemplateId = 103,
                    CreateDate = DateTime.Now,
                    Owner = new Player() { Name = "Faker" }
                }
            };

            db.Items.AddRange(items);
            db.SaveChanges();
        }
    }
}
