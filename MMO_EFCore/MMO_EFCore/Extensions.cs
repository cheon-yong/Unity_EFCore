using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMO_EFCore
{
    public static class Extensions
    {
        // IEnumerable (LINQ to Object / LINQ to XML 쿼리)
        // IQueryable (LINQ to SQL 쿼리)
        public static IQueryable<GuildDto> MapGuildToDto(this IQueryable<Guild> guild)
        {
            return guild.Select(g => new GuildDto
            {
                Name = g.GuildName,
                MemberCount = g.Members.Count
            });
        }
    }
}
