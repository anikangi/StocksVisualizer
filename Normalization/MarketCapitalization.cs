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
    
    public partial class MarketCapitalization
    {
        public long ID { get; set; }
        public long StockID { get; set; }
        public long MarketCap { get; set; }
        public System.DateTime Date { get; set; }
    
        public virtual Stock Stock { get; set; }
    }
}
