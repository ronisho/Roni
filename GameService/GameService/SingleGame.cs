//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GameService
{
    using System;
    using System.Collections.Generic;
    
    public partial class SingleGame
    {
        public int Id { get; set; }
        public string Winner { get; set; }
        public System.DateTime Date { get; set; }
        public bool Status { get; set; }
        public int GamePoint { get; set; }
        public string Player1_Name { get; set; }
        public string Player2_Name { get; set; }
    
        public virtual User User { get; set; }
        public virtual User User1 { get; set; }
    }
}