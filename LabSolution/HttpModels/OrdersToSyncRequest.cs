using System.Collections.Generic;

namespace LabSolution.HttpModels
{
    public class OrdersToSyncRequest
    {
        /// <summary> The list of Processed OrdersIds to be synched with Gov </summary>
        public List<int> ProcessedOrderIds { get; set; }
    }
}
