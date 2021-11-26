using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;

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

                string command =
                    @" CREATE FUNCTION GetAverageReviewScore (@itemId INT) RETURNS FLOAT
                       AS
                       BEGIN

                       DECLARE @result AS FLOAT

                       SELECT @result = AVG(CAST([Score] AS FLOAT))
                       FROM ItemReview AS r
                       WHERE @itemId = r.ItemId

                       RETURN @result    
                    
                       END";

                db.Database.ExecuteSqlRaw(command);

                CreateTestData(db);
                Console.WriteLine("DB Initialized");
            }
        }


        public static void CreateTestData(AppDbContext db)
        {
            var yj = new Player() { Name = "yj" };
            var faker = new Player() { Name = "Faker" };
            var deft = new Player() { Name = "Deft" };

            List<Item> items = new List<Item>()
            {
                new Item()
                {
                    TemplateId = 101,
                    Owner = yj
                }
            };

            // Test Shadow Property Value Write
            //db.Entry(items[0]).Property("RecoveredDate").CurrentValue = DateTime.Now;

            // Test Backing Field
            //items[0].SetOption(new ItemOption() { dex = 1, hp = 2, str = 3 });

            // Test Owned Type
            //items[0].Option = new ItemOption() { Dex = 1, Hp = 2, Str = 3 };

            //items[2].Detail = new ItemDetail()
            //{
            //    Description = "This is good Item"
            //};

            //// Backing Field + Relationship
            //items[0].AddReview(new ItemReview() { Score = 5 });
            //items[0].AddReview(new ItemReview() { Score = 4 });
            //items[0].AddReview(new ItemReview() { Score = 1 });
            //items[0].AddReview(new ItemReview() { Score = 5 });

            //items[0].Reviews = new List<ItemReview>()
            //{
            //    new ItemReview() { Score = 5 },
            //    new ItemReview() { Score = 3 },
            //    new ItemReview() { Score = 2 }
            //};
            
            //items[1].Reviews = new List<ItemReview>()
            //{
            //    new ItemReview() { Score = 1 },
            //    new ItemReview() { Score = 1 },
            //    new ItemReview() { Score = 0 }
            //};

            Guild guild = new Guild()
            {
                GuildName = "T1",
                Members = new List<Player>() { yj, faker, deft }
            };

            db.Items.AddRange(items);
            db.Guilds.Add(guild);

            Console.WriteLine("1번 : " + db.Entry(yj).State);
            
            db.SaveChanges();

            // Add Test
            {
                Item item = new Item()
                {
                    TemplateId = 500,
                    Owner = yj
                };
                db.Items.Add(item);
                // 아이템 추가 -> 간접적으로 player 영향
                // player는 Tracking 상태이고 Fk 설정은 필요 없음
                Console.WriteLine("2번 : " + db.Entry(yj).State);
            }

            // Delete Test
            {
                Player p = db.Players.First();

                // 위에서 아이템이 이미 DB에 들어간 상태(DB 키 있음)
                p.Guild = new Guild() { GuildName = "삭제될 길드" };
                p.OwnedItem = items[0];

                db.Players.Remove(p);

                // Player를 직접적으로 삭제하니까
                Console.WriteLine("3번 : " + db.Entry(p).State); // Deleted
                Console.WriteLine("4번 : " + db.Entry(p.Guild).State); // Added
                Console.WriteLine("5번 : " + db.Entry(p.OwnedItem).State); // Deleted
            }

            db.SaveChanges();
        }

        // 1) Tracking Entity를 획득
        // 2) Remove 호출
        // 3) SaveChanges 호출

        //public static void TestDelete()
        //{
        //    ShowItems();

        //    Console.WriteLine("Select Delete ItemId");
        //    Console.Write(" > ");
        //    int id = int.Parse(Console.ReadLine());

        //    using (AppDbContext db = new AppDbContext())
        //    {
        //        Item item = db.Items.Find(id);
        //        //db.Items.Remove(item);
        //        item.SoftDeleted = true;
        //        db.SaveChanges();
        //    }

        //    Console.WriteLine("--- TestDelete Complete ---");
        //    ShowItems();
        //}

        // RelationShip 복습
        // - Principal Entity (주요 -> Player)
        // - Dependent Entity (의존적 -> FK 포함하는 쪽 -> item)

        // 오늘의 주제
        // Dependent 데이터가 Principal 데이터 없이 존재할 수 있는가?
        // - 1) 주인이 없는 아이템은 불가능!
        // - 2) 주인이 없는 아이템도 가능! (ex. 로그 차원에서 남기는 경우)

        // 그러면 2 케이스 어떻게 구분해서 설정을 해야할까??
        // 답은 Nullable ! ex)int?
        // FK 그냥 int로 설정하면 1번, Nullable으로 설정하면 2번

        // 1) FK가 Nullable이 아니라면
        // - Player가 지워지면, FK로 해당 Player 참조하는 Item도 같이 삭제됨
        // 2) FK가 Nullable이라면
        // - Player가 지워지더라도 FK로 해당 Player 참조하는 Item은 그대로

        // 오늘의 주제
        // - 직접 State를 조작할 수 있다 (ex. 최적화 등)
        // ex) Entry().State = EntityState.Added
        // ex) Entry().Property("").IsModified = true


        public static void ShowItems()
        {
            using (AppDbContext db = new AppDbContext())
            {
                foreach (var item in db.Items.Include(i => i.Owner).IgnoreQueryFilters().ToList())
                {
                    
                    if (item.SoftDeleted)
                    {
                        Console.WriteLine($"DELETED - ItemId({item.ItemId}) TemplateId({item.TemplateId}) Owner({item.Owner.Name})");
                    }
                    else
                    {
                        if (item.Owner == null)
                            Console.WriteLine($"ItemId({item.ItemId}) TemplateId({item.TemplateId}) Owner(0)");
                        else
                            Console.WriteLine($"ItemId({item.ItemId}) TemplateId({item.TemplateId}) Owner({item.Owner.Name})");
                    }   
                }
            }
        }

        public static void TestUpdateAttach()
        {
            // Update Test
            using (AppDbContext db = new AppDbContext())
            {
                {
                    // Disconnected
                    Player p = new Player();
                    p.PlayerId = 2;
                    p.Name = "FankerSense";
                    // 아직 DB는 이 새로운 길드의 존재도 모름
                    p.Guild = new Guild { GuildName = "Udpate Guild" };

                    Console.WriteLine("6번)" + db.Entry(p.Guild).State);
                    db.Players.Update(p);
                    Console.WriteLine("7번)" + db.Entry(p.Guild).State);
                }

                {
                    Player p = new Player();
                    p.PlayerId = 3;
                    p.Name = "Deft-_-";

                    p.Guild = new Guild { GuildName = "Attach Guild" };

                    Console.WriteLine("8번)" + db.Entry(p.Guild).State);
                    db.Players.Attach(p);
                    Console.WriteLine("9번)" + db.Entry(p.Guild).State);
                }

                db.SaveChanges();
            }
      
        }

        //public static void CalcAverage()
        //{
        //    using (AppDbContext db = new AppDbContext())
        //    {
        //        foreach(double? avarage in db.Items.Select(i => Program.GetAverageReviewScore(i.ItemId)))
        //        {
        //            if (avarage == null)
        //                Console.WriteLine("No Review!");
        //            else
        //                Console.WriteLine($"Average : {avarage.Value}");
        //        }
        //    }
        //}

        //public static void Update_1v1()
        //{
        //    ShowItems();

        //    Console.WriteLine("Input ItemSwich PlayerId");
        //    Console.Write(" > ");
        //    int id = int.Parse(Console.ReadLine());

        //    using (AppDbContext db = new AppDbContext())
        //    {
        //        Player player = db.Players
        //            .Include(p => p.Item)
        //            .Single(p => p.PlayerId == id);

        //        if (player.Item != null)
        //        {
        //            player.Item.TemplateId = 888;
        //            player.Item.CreateDate = DateTime.Now;
        //        }

        //        //player.Item = new Item()
        //        //{
        //        //    TemplateId = 777,
        //        //    CreateDate = DateTime.Now
        //        //};

        //        db.SaveChanges();
        //    }

        //    Console.WriteLine("--- Test Complete ---");
        //    ShowItems();
        //}

        //public static void Update_1vM()
        //{
        //    ShowGuilds();

        //    Console.WriteLine("Input GuildId");
        //    Console.Write(" > ");
        //    int id = int.Parse(Console.ReadLine());

        //    using (AppDbContext db = new AppDbContext())
        //    {
        //        Guild guild = db.Guilds
        //            .Include(g => g.Members) //-> Include하면 덮어써짐
        //            .Single(g => g.GuildId == id);

        //        //guild.Members.Add(new Player()
        //        //{
        //        //    Name = "Dopa"
        //        //});

        //        //guild.Members = new List<Player>()
        //        //{
        //        //    new Player() { Name = "Keria" }
        //        //};

        //        guild.Members.Add(new Player() { Name = "Keria" });

        //        db.SaveChanges();
        //    }

        //    Console.WriteLine("--- Test Complete ---");
        //    ShowGuilds();
        //}

        //public static void Test()
        //{
        //    ShowItems();

        //    Console.WriteLine("Input Delete PlayerId");
        //    Console.Write(" > ");
        //    int id = int.Parse(Console.ReadLine());

        //    using (AppDbContext db = new AppDbContext())
        //    {
        //        Player player = db.Players
        //            .Include(p => p.Item)
        //            .Single(p => p.PlayerId == id);

        //        db.Players.Remove(player);
        //        db.SaveChanges();
        //    }

        //    Console.WriteLine("--- Test Complete ---");
        //    ShowItems();
        //}


        //// 1 + 2) 특정 길드에 있는 길드원들이 소지한 모든 아이템들을 보고 싶다!

        //// EagerLoading
        //// 장점 : 한번의 DB 접근으로 모두 로딩 (JOIN)
        //// 단점 : 모두가 다 필요한지 모른 채 로딩
        //public static void EagerLoading()
        //{
        //    Console.WriteLine("길드 이름을 입력하세요");
        //    Console.Write(" > ");
        //    string name = Console.ReadLine();

        //    using (var db = new AppDbContext())
        //    {
        //        Guild guild = db.Guilds.AsNoTracking()
        //            .Where(g => g.GuildName == name)
        //            .Include(g => g.Members)
        //                .ThenInclude(p => p.Item)
        //            .First();

        //        // AsNoTracking : ReadOnly << Tracking SnapShot이라고 데이터 변경 탐지하는 기능 때문
        //        // Include : Eager Loading (즉시 로딩) << 나중에 알아볼 것
        //        foreach (Player player in guild.Members)
        //        {
        //            Console.WriteLine($"TemplateId({player.Item.TemplateId}) Owner({player.Name})");
        //        }
        //    }
        //}

        //// ExplicitLoding
        //// 장점 : 원하는 시점에 원하는 데이터만 로딩 가능
        //// 단점 : DB 접근 비용
        //public static void ExplicitLoading()
        //{
        //    Console.WriteLine("길드 이름을 입력하세요");
        //    Console.Write(" > ");
        //    string name = Console.ReadLine();

        //    using (var db = new AppDbContext())
        //    {
        //        Guild guild = db.Guilds
        //            .Where(g => g.GuildName == name)
        //            .First();

        //        db.Entry(guild).Collection(g => g.Members).Load();

        //        foreach (Player player in guild.Members)
        //        {
        //            db.Entry(player).Reference(p => p.Item).Load();
        //        }

        //        foreach (Player player in guild.Members)
        //        {
        //            Console.WriteLine($"TemplateId({player.Item.TemplateId}) Owner({player.Name})");
        //        }
        //    }
        //}

        //// 3) 특정 길드에 있는 길드원 수는?

        //// SelectLoading
        //// 장점 : 필요한 정보만 로딩가능
        //// 단점 : 일일이 Select문 안에 입력해주어야함
        //public static void SelectLoading()
        //{
        //    Console.WriteLine("길드 이름을 입력하세요");
        //    Console.Write(" > ");
        //    string name = Console.ReadLine();

        //    using (var db = new AppDbContext())
        //    {
        //        var info = db.Guilds
        //            .Where(g => g.GuildName == name)
        //            .MapGuildToDto()
        //            .First();

        //        Console.WriteLine($"GuildName({info.Name}), MemberCound({info.MemberCount})");
        //    }
        //}

        //// Update 3단계
        //// 1) Tracked Entity를 얻어 온다
        //// 2) Entity 클래스의 property를 변경 (set)
        //// 3) SaveChanges 호출

        //// Update를 할 때 전체 수정을 하는 것일까?
        //// 수정해야할 부분만 수정할까?

        //// 1) SaveChanges 호출 할 때 -> 내부적으로 DetectChanges라는 호출
        //// 2) DetectChange에서 -> 최초 Snapshot / 현재 Snapshot 비교

        ///*
        // * SELECT TOP(2) GuildId, GuildName
        // * FROM [Guilds]
        // * WHERE GuildName = N'T1';
        // * 
        // * SET NOCOUNT ON;
        // * UPDATE [Guilds]
        // * SET GuildName = @p0
        // * WHERE GuildId = @p1;
        // * 
        // * SELECT @@ROWCOUNT;
        // * 
        // * 
        // */

        //// 오늘의 주제 : (Connected vs Disconnected) Update
        //// Disconnected : Update 단계가 한 번에 쭉~ 일어나지 않고, 끊기는 경우
        //// (REST API 등)

        //// 처리하는 2가지 방법
        //// 1) Reload 방식 : 필요한 정보만 보내서 1-2-3 스텝을 다시 시작
        //// 2) Full Update 방식 : 모든 정보를 다 보내고 받아서, 아예 Entity를 다시 만들어 Update

        //public static void ShowGuilds()
        //{
        //    using (AppDbContext db = new AppDbContext())
        //    {
        //        foreach (var guild in db.Guilds.MapGuildToDto())
        //        {
        //            Console.WriteLine($"GuildId({guild.GuildId}) GuildName({guild.Name}) MemberCount({guild.MemberCount}) ");
        //        }
        //    }
        //}

        //// 장점 : 최소한의 정보로 Update 가능
        //// 단점 : Read 두 번 한다
        //public static void UpdateByReload()
        //{
        //    ShowGuilds();

        //    // 외부에서 수정 원하는 데이터의 ID / 정보 넘겨줬다고 가정
        //    Console.WriteLine("Input GuildId");
        //    Console.Write(" > ");
        //    int id = int.Parse(Console.ReadLine());
        //    Console.WriteLine("Input GuildName");
        //    Console.Write(" > ");
        //    string name = Console.ReadLine();

        //    using (AppDbContext db = new AppDbContext())
        //    {
        //        Guild guild = db.Find<Guild>(id);
        //        guild.GuildName = name;
        //        db.SaveChanges();
        //    }

        //    Console.WriteLine("--- Update Complete ---");
        //    ShowGuilds();
        //}

        //public static string MakeUpdateJsonStr()
        //{
        //    var jsonStr = "{\"GuildId\" : 1, \"GuildName\" : \"Hello\", \"Member\":null}";
        //    return jsonStr;
        //}

        //// 장점 : DB에 다시 Read할 필요 없이 바로 Update
        //// 단점 : 모든 정보가 필요, 보안 문제
        //public static void UpdateByFull()
        //{
        //    ShowGuilds();

        //    //string jsonStr = MakeUpdateJsonStr();
        //    //Guild guild = JsonConvert.DeserializeObject<Guild>(jsonStr);

        //    Guild guild = new Guild()
        //    {
        //        GuildId = 1,
        //        GuildName = "TestGuild"
        //    };

        //    using (AppDbContext db = new AppDbContext())
        //    {
        //        db.Guilds.Update(guild);
        //        db.SaveChanges();
        //    }

        //    Console.WriteLine("--- Update Complete ---");
        //    ShowGuilds();
        //}
    }
}
