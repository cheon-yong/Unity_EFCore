using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace MMO_EFCore
{
    // 오늘의 주제 (21-11-23) : Configuration


    // Data Modeling Configuration
    // A) Convention (관례)
    // - 각종 형식과 이름 등을 정해진 규칙에 맞게 만들면, EF Core에서 알아서 처리
    // - 쉽고 빠르지만, 모든 경우를 처리할 수 없음

    // B) Data Annotation (데이터 주석)
    // - class/property 등에 Attribute를 붙여 추가 정보

    // C) Fluent Api (직접 정의)
    // - OnModelCreating에서 그냥 직접 설명을 정의하여 만드는 방식
    // - 가장 활용 범위가 넓음

    // ------------- Convention --------------
    // 1) Entity Class 관련
    // - public 접근 한정자 + non-static
    // - property 중에서 public getter를 찾으면서 분석
    // - property 이름 = table column 이름
    // 2) 이름, 형식, 크기 관련
    // - .NET 형식 <-> SQL 형식 (int, bool)
    // - .NET 형식의 Nullable 여부를 따라감 (string은 nullable, int non-null(int?))
    // 3) PK 관련
    // - Id 혹은 <클래스이름>Id 정의된 property는 PK로 인정 (후자 권장)
    // - 복합키(Composite Key) Convention으로 처리 불가
    // ----------------------------------------

    // Q1) DB Column type, size, nullable
    // Nullable      [Required]     .IsRequired()
    // 문자열길이     [MaxLength(20] .HasMaxLength(20)
    // 문자 형식                     .IsUnicode(true)

    // Q2) PK
    // [Key][Column(Order=0)] [key][Column(Order=1)]
    // .HasKey(x => new {x.Prop1, x.Prop2})

    // Q3) Index
    // 인덱스 추가                   .HasIndex(p => p.Prop1)
    // 복합 인덱스 추가               .HasIndex(p => new { p.Prop1, p.Prop2 })
    // 인덱스 이름을 정해서 추가      .HasIndex(p => p.Prop1).HasName("Index_MyProp")
    // 유니크 인덱스 추가             .HasIndex(p => p.Prop1).IsUnique()

    // Q4) 테이블 이름
    // DBSet<T> property 이름 or class 이름
    // [Table("MyTable")]           .ToTable("MyTable")

    // Q5) 칼럼 이름
    // property 이름
    // [Column("MyCol")]             .HasColumnName("MyCol")

    // Q6) 코드 모델링에서는 사용하되, DB 모델링에서는 제외하고싶다면? (property / class 모두)
    // [NotMapped]

    // Q7) Soft Deleted
    // .HasQueryFilter()

    // 언제 무엇을?
    // 1) Convention이 가장 무난
    // 2) Validation과 관련된 부분들은 Data Annotation (직관적, SaveChanges 호출)
    // 3) 그 외에는 Fluent Api

    // DB 관계 모델링
    // 1:1
    // 1:다
    // 다:다


    // Relationship Configuration

    // 기본 용어 복습
    // 1) Principal Entity
    // 2) Dependent Entity
    // 3) Navigational Property
    // 4) Primary key (PK)
    // 5) Foreign key (FK)
    // 6) Principal Key = PK or Unique Alternative key
    // 7) Required Relationship (Not-Null)
    // 8) Optional Relationship (Nullable)


    // Convention을 이용한 FK 설정
    // 1) <PrincipalKeyName>                                 PlayerId
    // 2) <Class><PrincipalKeyName>                          PlayerPlayerId
    // 3) <NavigationalPropertyName><PrincipalKeyName>       OwnerPlayerId  OwnerId

    // FK 와 Nullable
    // 1) Required Relationship (Not-Null)
    // 삭제할 때 OnDelete 인자를 Cascade 모드로 호출 -> Principal 삭제하면 Dependent 삭제
    // 2) Optional Relationship (Nullable)
    // 삭제할 때 OnDelete 인자를 ClientSetNull 모드로 호출
    // -> Pricipal 삭제할 때 Dependent Tracking하고 있으면, FK 를 null 세팅
    // -> Pricipal 삭제할 때 Dependent Tracking하고 있지 않으면, Exception 발생

    // Convention 방식으로 못하는 것들
    // 1) 복합 키
    // 2) 다수의 Navigational Property가 같은 클래스를 참조할 때
    // 3) DB나 삭제 관련 커스터마이징 필요할 때

    // Data Annotation으로 Relationship 설정
    // [ForeignKey("Prop1")]
    // [InverseProperty] -> 다수의 Navigational Property가 같은 클래스를 참조할 때

    // Fluent Api로 Relationship 설정
    // .HasOne() .HasMany()
    // .WithOne() .WithMany()
    // .HasForeignKey() .IsRequired() .OnDelete()
    // .HasContrainName() .HasPrincipalKey()


    // Shadow Property & Backing Field

    // Shadow Property
    // Class에는 있지만 DB에는 없음 -> [NotMapped] .Ignore()
    // DB에는 있지만 Class에는 없음 -> Shadow Property
    // 생성 -> .Property<DateTime>("UpdatedOn")
    // Read/Write -> .Property("RecoveredDate").CurrentValue

    // Backing Field
    // private property를 DB에 매핑하고, public getter로 가공해서 사용
    // ex) DB에는 json 형태로 string을 저장하고, getter은 json을 가공해서 사용
    // 일반적으로 Fluent Api

    // 오늘의 주제 : Entity <-> DB Table 연동하는 다양한 방법들
    // Entity Class 하나를 통으로 Read/Writh -> 부담 (Select Loading, DTO)

    // 1) Owned Type
    // - 일반 클래스를 Entity Class에 추가하는 개념
    // a) 동일한 테이블 추가
    // - .OwnsOne()
    // - RelationShip이 아니라 Ownership의 개념이기 때문에 자동 .Include()
    // b) 다른 테이블에 추가
    // - .OwnsOne().ToTable()

    // 2) Table Per Hierarchy (TPH)
    // - 상속 관계의 여러 class <-> 하나의 테이블에 매핑
    // ex) Dog, Cat, Bird -> Animal
    // a) Convention
    // - 일단을 Class를 상속받아 만들고 DBSet에 추가
    // - Discriminator ?
    // b) Fluent Api
    // - .HasDiscriminator 

    // 3) Table Splitting
    // - 다수의 Entity Class <-> 하나의 테이블에 매핑


    // 오늘의 주제 : Backing Field + Relationship
    // Backing Field -> private field를 DB에 매핑
    // Navigation Property 에서도 사용 가능!

    // User Defined Function (UDF)
    // 우리가 직접 만든 SQL을 호출하게 하는 기능
    // - 연산을 DB쪽에서 하도록 떠넘기고 싶다
    // - EF Core 쿼리가 약간 비효율적이다

    // Step
    // 1) Configuration
    // - static 함수를 만들고 EF Core 등록
    // 2) DataBase Setup
    // 3) 사용

    // 초기값 (Default Value)
    // 기본값을 설정하는 방법이 여러가지가 있다
    // 주의해서 볼 것!
    // 1) Entity Class 자체의 초기값으로 붙는지
    // 2) DB Table 차원에서 초기값으로 적용되는지
    // - 결과는 같은거 아닐까?
    // - EF <-> DB 외에 다른 경로로 DB 사용한다면, 차이가 날 수 있다.
    // ex) SQL Script

    // 1) Auto-Property Initializer (C# 6.0)
    // - Entity 차원의 초기값 -> SaveChanges로 DB 적용
    // 2) Fluent Api
    // - DB Table DEFAULT를 적용
    // - DateTime.Noew ? 처리 힘듦
    // 3) SQL Fragment (새로운 값이 추가되는 시점에 DB쪽에서 실행)
    // - .HasDefaultValueSql
    // 4) Value Generator (EF Core 에서 실행됨)
    // - 일종의 Generator 규칙

    // Migration
    // 일단 EF Coree DbContext <-> DB 상태에 대해 동의가 있어야 함
    // 무엇을 기준으로 할 것인가?

    // 1) Code-First
    // - 지금까지 우리가 사용하던 방식 (Entity Class / DbContext가 기준)
    // - 항상 최신 상태로 DB를 업데이트 하고 싶다는 의미가 아님

    // +++ Migration Step +++
    // A) Migration 만들고
    // B) Migration 적용하고

    // A) Add-Migration [Name]
    // - 1) DbContext를 찾아서 분석 -> DB 모델링 (최신)
    // - 2) ~ModelSnapshot.cs을 이용하여 가장 마지막 Migration 상태의 DB 모델링 (가장 마지막 상태)
    // - 3) 1-2 비교 결과 도출
    // -- a) ModelSnapshot
    // -- b) Migrate.Designer.cs와 Migrate.cs -> Migration과 관련된 세부정보
    // 수동으로 Up, Down을 추가해도 됨

    // B) Migration 적용
    // - 1) SQL change script
    // -- Script-Migration [From] [To] [Options]
    // - 2) Database.Migarte 호출
    // - 3) Command Line 방식
    // - Update-Database [options]

    // 특정 Migration으로 Sync (Update-database [Name])
    // 마지막 Migration 삭제 (Remove-Migration)

    // 2) Database-First

    // 3) SQL-First
    // -- 손수 만들어도 됨
    // -- Script-Migration [From] [To] [Options]
    // -- DB끼리의 비교를 이용하여 SQL 추출

    // 오늘의 주제 : DbContext 심화 과정 (최적화 등 때문에)
    // 1) ChangeTracker
    // - Tracking State 관련
    // 2) Database
    // - Transaction
    // - DB Creation/Migration
    // - Raw SQL
    // 3) Model
    // - DB 모델링 관련

    // State 관리
    // 0) Detached (No Tracking ! 추적되지 않는 상태. SaveChanges를 해도 존재조차 모름)
    // 1) Unchanged (DB에는 이미 존재, 딱히 수정사항도 없었음. SaveChanges를 해도 아무 변경도 X)
    // 2) Deleted (DB에는 아직 있지만, 삭제되어야 함. SaveChanges로 DB에 적용)
    // 3) Modified (DB에 존재하고 클라이언트에서 수정된 상태. SaveChanges로 적용)
    // 4) Added (DB에는 아직 존재하지 않음. SaveChanges로 적용)

    // State 체크 방법
    // - Entry().State
    // - Entry().Property().IsModified
    // - Entry().Navigation().IsModified

    // State 가 대부분 '직관적/ 이지만 Realtionship이 개입하면 살짝 더 복잡해짐
    // - 1) Add/AddRange 사용할 때의 상태 변화
    // -- NotTracking 상태라면 Added
    // -- Tracking 상태인데, Fk 설정이 필요한데 따라 Modified / 기존 상태 유지
    // - 2) Remove/RemoveRange 사용할 떄의 상태 변화
    // - (DB에 의해 생성된 Key) && (C# 기본값 아님) -> 필요에 따라 Unchanged / Modified / Deleted
    // - (DB에 의해 생성된 Key없음) || (C# 기본값) - > Added
    // -- 삭제하는데 왜 굳이 Added인지? 동작의 일관성 때문
    // -- DB에서도 일단 존재는 알아야 Cascade Delete 처리 
    //
    // - 3) Update/UpdateRange
    // - EF에서 Entity를 Update하는 기본적인 방법은 Update가 아님
    // - Tracked Entity를 얻어오고 -> property 수정 -> SaveChanges
    // - Update는 Uptracked Entity를 통으로 업데이트 할 때 (Disconnected State);

    // EF Core에서 Update하면 일어나는 Step
    // 1) Update 호출
    // 2) Entity State = Modified로 변경
    // 3) 모든 non-Relational Property의 IsModified = true 로 변경

    // - 4) Attach
    // - Untracked Entity를 Tracked Entity로 변경

    // Relationship
    // - (DB에 의해 생성된 Key) && (0 아님) -> Unchaged
    // - (DB에 의해 생성된 Key 없음) || 0)-> Added

    // Entity 클래스 이름 = 테이블 이름 = item

    [Table("Items")]
    public class Item
    {
        public bool SoftDeleted { get; set; }
        // 이름Id -> PK
        public int ItemId { get; set; }
        public int TemplateId { get; set; }
        public DateTime CreateDate { get; private set; }

        public int ItemGrade { get; set; }

        // 다른 클래스 참조 -> FK (Navigational Property)
        // [ForeignKey("OwnerId")]
        public int OwnerId { get; set; }
        public Player Owner { get; set; }

    }

    // 클래스 이름 = 테이블 이름 = player
    [Table("Player")]
    public class Player
    {
        // 이름Id -> PK
        public int PlayerId { get; set; }
        [Required]
        [MaxLength(20)]
        public string Name { get; set; }

        public Item OwnedItem { get; set; }
        public Guild Guild { get; set; }
    }

    [Table("Guild")]
    public class Guild
    {
        public int GuildId { get; set; }
        public string GuildName { get; set; }
        public ICollection<Player> Members { get; set; }
    }

    // DTO (Data Transfer Object)
    public class GuildDto
    {
        public int GuildId { get; set; }
        public string Name { get; set; }
        public int MemberCount { get; set; }
    }

}