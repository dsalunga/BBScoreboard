//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BBScoreboard
{
    using System;
    using System.Collections.Generic;
    
    public partial class UCGame
    {
        public int Id { get; set; }
        public int GameNumber { get; set; }
        public int Team1 { get; set; }
        public int Team2 { get; set; }
        public System.DateTime GameDate { get; set; }
        public int CurrentQuarter { get; set; }
        public int SeasonId { get; set; }
        public string Venue { get; set; }
        public bool IsStarted { get; set; }
        public System.DateTime TimeLastModified { get; set; }
        public bool IsTimeOn { get; set; }
        public System.DateTime TimeLeft { get; set; }
        public System.DateTime LastActivityDate { get; set; }
        public System.DateTime LastUpdate { get; set; }
        public System.DateTime LastUpdateForRefresh { get; set; }
        public bool IsEnded { get; set; }
    }
}
