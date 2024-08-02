using Domain.Enums.Others;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
#pragma warning disable CS8618
    [Index(nameof(Phone), IsUnique = true)]
    [Index(nameof(ProviderId), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    public class Account
    {
        public int Id { get; set; }
        [Column(TypeName = "citext")]
        public string Name { get; set; }
        public string? Phone { get; set; }
        [Column(TypeName = "citext")]
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public bool IsMale { get; set; }
        public decimal GcoinBalance { get; set; }
        public int PrestigePoint { get; set; }
        public string? Address { get; set; }
        [Column(TypeName = "geography (point)")]
        public Point? Coordinate { get; set;}
        public string? AvatarPath { get; set; }
        public string? DeviceToken { get; set; }
        public DateTime CreatedAt { get; set; }
        public Role Role { get; set; }
        public bool IsActive { get; set; } = true;
        //one - one
        public int? ProviderId { get; set; }
        public virtual Provider? Provider { get; set; }
        //one - many
        public virtual HashSet<Announcement> Announcements { get; set; }
        public virtual HashSet<Transaction>? Transactions { get; set; }
        public virtual HashSet<Plan> Plans { get; set; }
        public virtual HashSet<Order> Orders { get; set; }
        public virtual HashSet<PlanMember> PlanMembers { get; set; }
        public virtual HashSet<DestinationComment> DestinationComments { get; set; }
        //public HashSet<PlanRating>? PlanRatings { get; set; }
    }
}
