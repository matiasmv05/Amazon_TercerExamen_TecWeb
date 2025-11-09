using Amazon.infrastructure.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Infrastructure.DTOs
{
    public class PaymentDto
    {
        public int? Id { get; set; }
        public int OrderId { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual OrderDto? order { get; set; }
    }
}
