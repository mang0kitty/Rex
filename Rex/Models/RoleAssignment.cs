using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Rex.Models
{
    public class RoleAssignment
    {
        public string UserId { get; set; }

        public string CollectionId { get; set; }

        public RoleType Role { get; set; }
    }

    public enum RoleType
    {
        Owner,
        Creator,
        Viewer
    }
}