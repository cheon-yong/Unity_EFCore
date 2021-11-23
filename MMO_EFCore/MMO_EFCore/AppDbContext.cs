using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace MMO_EFCore
{
    // EF Core 작동 스텝
    // 1) DbContext 만들 때
    // 2) DbSet<T>를 찾는다
    // 3) 모델링 class 분석해서, 칼럼을 찾는다.
    // 4) 모델링 class에서 참조하는 다른 class가 있으면, 걔도 분석한다.
    // 5) OnModelCreating 함수 호출 (추가 설정 = override)
    // 6) 데이터베이스의 전체 모델링 구조를 내부 메모리에 들고 있음

    public class AppDbContext : DbContext
    {
        // DbSet<Item> -> EF Core한테 알려준다.
        // items이라는 DB 테이블이 있는데, 세부적인 칼럼/키 정보는 Item 클래스를 참고
        public DbSet<Item> Items { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Guild> Guilds { get; set; }

        // DB ConnectionString
        // 어떤 DB를 어떻게 연결하라
        public const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EfCoreDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(ConnectionString);

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Entity에 접근할 때 항상 사용되는 모델 레벨의 필터링
            // 필터를 무시하고 싶으면 IgnoreQueryFilters 옵션 추가

            builder.Entity<Item>().HasQueryFilter(i => i.SoftDeleted == false);

            builder.Entity<Player>()
                .HasIndex(p => p.Name)
                .HasName("Index_Person_name")
                .IsUnique();

            builder.Entity<Player>()
                .HasMany(p => p.CreatedItems)
                .WithOne(i => i.Creator) // 상대쪽
                .HasForeignKey(i => i.CreatorId); // 1:M인 경우에는 M인 쪽에 FK

            builder.Entity<Player>()
                .HasOne(p => p.OwnedItem)
                .WithOne(i => i.Owner)
                .HasForeignKey<Item>(i => i.OwnerId);
        }
    }
}
