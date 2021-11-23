using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

    // Entity 클래스 이름 = 테이블 이름 = item

    public struct ItemOption
    {
        public int str;
        public int dex;
        public int hp;
    }

    [Table("Item")]
    public class Item
    {
        private string _jsonData;
        public string JsonData 
        {
            get { return _jsonData; }
        }

        public void SetOption(ItemOption option)
        {
            _jsonData = JsonConvert.SerializeObject(option);
        }

        public ItemOption GetOption()
        {
            return JsonConvert.DeserializeObject<ItemOption>(_jsonData);
        }

        public bool SoftDeleted { get; set; }
        // 이름Id -> PK
        public int ItemId { get; set; }
        public int TemplateId { get; set; }
        public DateTime CreateDate { get; set; }

        // 다른 클래스 참조 -> FK (Navigational Property)
        // [ForeignKey("OwnerId")]
        public int OwnerId { get; set; }
        public Player Owner { get; set; }
        
        public int? CreatorId { get; set; }
        public Player Creator { get; set; }
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
        public ICollection<Item> CreatedItems { get; set; }

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
