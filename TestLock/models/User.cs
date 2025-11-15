using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForeverProjects.models
{
    [Table("P_Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = default!;

        // 反向导航（可选）
        // 导航属性只是入口，EF 不会主动 JOIN。仅当：
        // 1. 查询时显示 Include();
        // 2. 延迟加载（Lazy Loading，需要额外配置）
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
