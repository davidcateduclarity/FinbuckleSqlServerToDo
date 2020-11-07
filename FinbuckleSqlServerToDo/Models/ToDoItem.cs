using System.ComponentModel.DataAnnotations.Schema;

namespace FinbuckleSqlServerToDo.Models
{
    public class ToDoItem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Title { get; set; }

        public bool Completed { get; set; }
    }
}