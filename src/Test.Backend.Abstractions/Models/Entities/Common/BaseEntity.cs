using System.ComponentModel.DataAnnotations.Schema;

namespace Test.Backend.Abstractions.Models.Entities.Common
{
    public interface IEntity
    {

    }

    public abstract class BaseEntity : IEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTime? InsertDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
