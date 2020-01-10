using System;
namespace za.co.grindrodbank.a3s.Models
{
    public class SubRealm : AuditableModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Decription { get; set; }
    }
}
