using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace za.co.grindrodbank.a3s.Models
{
    [Table("RoleFunctionTransient")]
    public class RoleFunctionTransientModel : TransientStateMachineRecord
    {
        [Required]
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid RoleId { get; set; }
        [Required]
        public Guid FunctionId { get; set; }
    }
}
