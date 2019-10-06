using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Rex.Models
{
    public class Collection
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Idea> Ideas { get; set; }

        public virtual ICollection<RoleAssignment> RoleAssignments { get; set; }
    }
}