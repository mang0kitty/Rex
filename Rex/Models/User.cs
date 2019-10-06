using System.Collections.Generic;

namespace Rex.Models
{
    public class User
    {
        public string Email { get; set; }

        public string PrincipalId { get; set; }

        public virtual Collection DefaultCollection { get; set; }

        public virtual ICollection<RoleAssignment> RoleAssignments { get; set; }
    }
}