using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FourinrowDB
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=fourinrowDB; AttachDbFilename=C:\fourinrow\databases\fourinrowDB_RoniShoseov_EilonOsher.mdf;Integrated Security=True";
            using (var ctx = new FourinrowContext(connectionString))
            {
                ctx.Database.Initialize(force: true);
            }
        }
    }
}
