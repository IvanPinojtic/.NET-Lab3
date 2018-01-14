using System;
using System.Collections.Generic;

namespace WebServis.DAL.Entities
{
    public partial class SalesPersonQuotaHistory
    {
        public int BusinessEntityId { get; set; }
        public DateTime QuotaDate { get; set; }
        public decimal SalesQuota { get; set; }
        public Guid Rowguid { get; set; }
        public DateTime ModifiedDate { get; set; }

        public SalesPerson BusinessEntity { get; set; }
    }
}
