using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MMO_EFCore
{
    [Table("Item")]
    public class Item
    {
        // 이름Id -> PK
        public int ItemId { get; set; }
        public int TemplateId { get; set; }
        public DateTime CreateDate { get; set; }

        // 다른 클래스 참조 -> FK (Navigational Property)
        public int OwnerId { get; set; }
        public Player Owner { get; set; }
    }


    // 클래스 이름 = 테이블 이름 = player
    public class Player
    {
        // 이름Id -> PK
        public int PlayerId { get; set; }
        public string Name { get; set; }
    }
}
