﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Normalization
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class NeeksDBEntities : DbContext
    {
        public NeeksDBEntities()
            : base("name=NeeksDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<HistoricalData> HistoricalDatas { get; set; }
        public virtual DbSet<MarketCapitalization> MarketCapitalizations { get; set; }
        public virtual DbSet<Stock> Stocks { get; set; }
        public virtual DbSet<Stocks_V> Stocks_V { get; set; }
    }
}