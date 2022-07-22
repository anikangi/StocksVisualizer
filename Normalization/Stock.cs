//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class Stock
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Stock()
        {
            this.HistoricalDatas = new HashSet<HistoricalData>();
            this.MarketCapitalizations = new HashSet<MarketCapitalization>();
        }
    
        public long ID { get; set; }
        public string Symbol { get; set; }
        public string Company { get; set; }
        public string Exchange { get; set; }
        public string Industry { get; set; }
        public string Description { get; set; }
        public string IssueType { get; set; }
        public Nullable<long> SectorID { get; set; }
        public Nullable<int> Employees { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HistoricalData> HistoricalDatas { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MarketCapitalization> MarketCapitalizations { get; set; }
    }
}