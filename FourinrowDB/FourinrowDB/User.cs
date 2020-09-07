using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FourinrowDB
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string UserName { get; set; }
        public string HashedPassword { get; set; }
        public int NumOfGames { get; set; }
        public int NumOfWins { get; set; }
        public int NumOfLosses { get; set; }
        public int Points { get; set; }
    }
}
