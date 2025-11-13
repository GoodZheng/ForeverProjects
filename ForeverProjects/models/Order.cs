using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForeverProjects.models
{
    [Table("P_Orders")]
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // 1. 外键列：非可空 + 级联删除（当父记录（User）被删除时，数据库自动把所有关联的子记录（Order）一起删掉，防止产生“孤儿”数据。）
        [ForeignKey(nameof(User))]   // 指向导航属性名
        public int UserId { get; set; }

        public User User { get; set; } = default!;

        // 2. 枚举 → 存字符串（取数据时当成枚举即可）
        [Column(TypeName = "varchar(20)")]
        public Status Status { get; set; }
    }

    public enum Status
    {
        Pending,
        Processing,
        Completed,
        Cancelled
    }
}
