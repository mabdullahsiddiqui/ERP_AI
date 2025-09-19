using System;
using System.Collections.Generic;

namespace ERP_AI.Data
{
    public class IndustryChartTemplate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Account> Accounts { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsSystem { get; set; } = false;
    }
}
