using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
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

                CreateTestData(db);
                Console.WriteLine("DB Initialized");
            }
        }

        public static void CreateTestData(AppDbContext db)
        {
            var yj = new Player() { Name = "Yj" };
            var faker = new Player() { Name = "Faker" };
            var deft = new Player() { Name = "Deft" };

            List<Item> items = new List<Item>()
            {
                new Item()
                {
                    TemplateId = 101,
                    CreateDate = DateTime.Now,
                    Owner = yj
                },
                new Item()
                {
                    TemplateId = 102,
                    CreateDate = DateTime.Now,
                    Owner = faker
                },
                new Item()
                {
                    TemplateId = 103,
                    CreateDate = DateTime.Now,
                    Owner = deft
                }
            };

            Guild guild = new Guild()
            {
                GuildName = "T1",
                Members = new List<Player>() { yj, faker, deft }
            };

            db.Items.AddRange(items);
            db.Guilds.Add(guild);
            db.SaveChanges();
        }

        // 1 + 2) 특정 길드에 있는 길드원들이 소지한 모든 아이템들을 보고 싶다!

        // EagerLoading
        // 장점 : 한번의 DB 접근으로 모두 로딩 (JOIN)
        // 단점 : 모두가 다 필요한지 모른 채 로딩
        public static void EagerLoading()
        {
            Console.WriteLine("길드 이름을 입력하세요");
            Console.Write(" > ");
            string name = Console.ReadLine();

            using (var db = new AppDbContext())
            {
                Guild guild = db.Guilds.AsNoTracking()
                    .Where(g => g.GuildName == name)
                    .Include(g => g.Members)
                        .ThenInclude(p => p.Item)
                    .First();

                // AsNoTracking : ReadOnly << Tracking SnapShot이라고 데이터 변경 탐지하는 기능 때문
                // Include : Eager Loading (즉시 로딩) << 나중에 알아볼 것
                foreach (Player player in guild.Members)
                {
                    Console.WriteLine($"TemplateId({player.Item.TemplateId}) Owner({player.Name})");
                }
            }
        }

        // ExplicitLoding
        // 장점 : 원하는 시점에 원하는 데이터만 로딩 가능
        // 단점 : DB 접근 비용
        public static void ExplicitLoading()
        {
            Console.WriteLine("길드 이름을 입력하세요");
            Console.Write(" > ");
            string name = Console.ReadLine();

            using (var db = new AppDbContext())
            {
                Guild guild = db.Guilds
                    .Where(g => g.GuildName == name)
                    .First();

                db.Entry(guild).Collection(g => g.Members).Load();
                
                foreach (Player player in guild.Members)
                {
                    db.Entry(player).Reference(p => p.Item).Load();
                }

                foreach (Player player in guild.Members)
                {
                    Console.WriteLine($"TemplateId({player.Item.TemplateId}) Owner({player.Name})");
                }
            }
        }

        // 3) 특정 길드에 있는 길드원 수는?

        // SelectLoading
        // 장점 : 필요한 정보만 로딩가능
        // 단점 : 일일이 Select문 안에 입력해주어야함
        public static void SelectLoading()
        {
            Console.WriteLine("길드 이름을 입력하세요");
            Console.Write(" > ");
            string name = Console.ReadLine();

            using (var db = new AppDbContext())
            {
                var info = db.Guilds
                    .Where(g => g.GuildName == name)
                    .MapGuildToDto()
                    .First();

                Console.WriteLine($"GuildName({info.Name}), MemberCound({info.MemberCount})");
            }
        }

       
    }
}
