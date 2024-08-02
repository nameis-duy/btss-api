using Application.DTOs.Generic;
using Domain.Entities;

namespace Application.DTOs.Traveler
{
#pragma warning disable CS8618
    public class TravelerCreateResult
    {
        public Account Account { get; set; }
        public AuthResult AuthResult { get; set; }
    }
}
