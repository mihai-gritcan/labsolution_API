namespace LabSolution.HttpModels
{
    public class ProcessedOrderSetPriceRequest
    {
        public int ProcessedOrderId { get; set; }
        public decimal Price { get; set; }
    }
}