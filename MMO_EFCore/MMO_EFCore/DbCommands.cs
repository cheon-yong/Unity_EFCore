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

        // State 관리
        // 0) Detached (No Tracking ! 추적되지 않는 상태. SaveChanges를 해도 존재조차 모름)
        // 1) Unchanged (DB에는 이미 존재, 딱히 수정사항도 없었음. SaveChanges를 해도 아무 변경도 X)
        // 2) Deleted (DB에는 아직 있지만, 삭제되어야 함. SaveChanges로 DB에 적용)
        // 3) Modified (DB에 존재하고 클라이언트에서 수정된 상태. SaveChanges로 적용)
        // 4) Added (DB에는 아직 존재하지 않음. SaveChanges로 적용)

        // SaveChanges 호출하면 어떤 일이?
        // 1) 추가된 객체들의 상태가 UnChanged로 변경
        // 2) SQL Identity로 PK를 관리
        //  - 데이터 추가 후 ID 받아와서 객체의 ID property를 채운다.
        //  - Relationship 참고해서, FK 세팅 및 객체 참조 연결

        // 이미 존재하는 사용자를 연동하려면?
        // 1) Tracked Instance (추적되고 있는 객체)를 얻어와서
        // 2) 데이터 연결
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

        // Update 3단계
        // 1) Tracked Entity를 얻어 온다
        // 2) Entity 클래스의 property를 변경 (set)
        // 3) SaveChanges 호출
        
        // Update를 할 때 전체 수정을 하는 것일까?
        // 수정해야할 부분만 수정할까?

        // 1) SaveChanges 호출 할 때 -> 내부적으로 DetectChanges라는 호출
        // 2) DetectChange에서 -> 최초 Snapshot / 현재 Snapshot 비교

        /*
         * SELECT TOP(2) GuildId, GuildName
         * FROM [Guilds]
         * WHERE GuildName = N'T1';
         * 
         * SET NOCOUNT ON;
         * UPDATE [Guilds]
         * SET GuildName = @p0
         * WHERE GuildId = @p1;
         * 
         * SELECT @@ROWCOUNT;
         * 
         * 
         */

        public static void UpdateTest()
        {
            using (AppDbContext db = new AppDbContext())
            {
                var guild = db.Guilds.Single(g => g.GuildName == "T1");

                guild.GuildName = "DWG";
                
                db.SaveChanges();
            }
        }
    }
}
