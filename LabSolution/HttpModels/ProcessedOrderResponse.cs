using System;

namespace LabSolution.HttpModels
{

    public class ProcessedOrderResponse
    {
        public int Id { get; set; }
        public int CustomerOrderId { get; set; }
        public long NumericCode { get; set; }
        public byte[] Barcode { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}
