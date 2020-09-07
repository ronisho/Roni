using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FourinrowDB
{
    public class SingleGame
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Winner { get; set; }
        public DateTime Date { get; set; }
        public bool Status { get; set; }
        public User Player1 { get; set; }
        public User Player2 { get; set; }
        public int GamePoint { get; set; }

    }
}
