using LabSolution.Dtos;
using LabSolution.HttpModels;
using LabSolution.Infrastructure;
using LabSolution.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LabSolution.Services
{
    public interface IOrderService
    {
        Task<List<CustomerOrder>> SaveOrders(CreateOrderRequest createOrder, IEnumerable<Customer> customersEntities);
        Task<List<DateTime>> GetOccupiedTimeSlots(DateTime date);
        Task<List<CreatedOrdersResponse>> GetCreatedOrders(DateTime date, long? idnp);
        Task<CustomerOrder> GetOrderDetails(int createdOrderId);
        Task UpdateOrder(UpdateOrderRequest updateOrderRequest);
        Task<ProcessedOrder> SaveProcessedOrder(int orderId, long numericCode, byte[] barcode);
        Task SetTestResult(int orderId, long numericCode, TestResult testResult);
        Task<List<FinishedOrderResponse>> GetFinishedOrders(DateTime date);
    }

    public class OrderService : IOrderService
    {
        private readonly LabSolutionContext _context;

        public OrderService(LabSolutionContext context)
        {
            _context = context;
        }

        public Task<List<DateTime>> GetOccupiedTimeSlots(DateTime date)
        {
            return _context.CustomerOrders.Where(x => x.Scheduled.Date == date.Date).Select(x => x.Scheduled).ToListAsync();
        }

        public Task<List<CreatedOrdersResponse>> GetCreatedOrders(DateTime date, long? idnp)
        {
            var queryableOrders = _context.CustomerOrders.Where(x => x.Scheduled.Date == date.Date && x.ProcessedOrder == null)
                .Include(x => x.Customer)
                .Select(x => new CreatedOrdersResponse
                {
                    Id = x.Id,
                    CustomerId = x.CustomerId,
                    Customer = CustomerDto.CreateDtoFromEntity(x.Customer, x.ParentId == null),
                    ParentId = x.ParentId,
                    Placed = x.Placed,
                    Scheduled = x.Scheduled,
                    PrefferedLanguage = (TestLanguage)x.PrefferedLanguage,
                    TestType = (TestType)x.TestType
                });

            if (idnp is not null)
                queryableOrders = queryableOrders.Where(x => x.Customer.PersonalNumber.ToString().Contains(idnp.Value.ToString()));

            return queryableOrders.ToListAsync();
        }

        public Task<List<FinishedOrderResponse>> GetFinishedOrders(DateTime date)
        {
            return _context.ProcessedOrders.Where(x => x.TestResult != null && x.ProcessedAt.Date == date.Date)
                .Include(x => x.CustomerOrder)
                .Select(x => new FinishedOrderResponse
                {
                    Id = x.Id,
                    TestResult = (TestResult)x.TestResult,
                    NumericCode = x.NumericCode,
                    OrderDate = x.CustomerOrder.Scheduled
                }).ToListAsync();
        }

        public Task<CustomerOrder> GetOrderDetails(int createdOrderId)
        {
            return _context.CustomerOrders.SingleOrDefaultAsync(x => x.Id == createdOrderId);
        }

        public async Task<List<CustomerOrder>> SaveOrders(CreateOrderRequest createOrder, IEnumerable<Customer> customersEntities)
        {
            var ordersToAdd = new List<CustomerOrder>();

            var rootCustomer = customersEntities.Single(x => x.PersonalNumber == createOrder.Customers.First(c => c.IsRootCustomer).PersonalNumber);

            foreach (var customer in createOrder.Customers)
            {
                var customerId = customersEntities.Single(x => x.PersonalNumber == customer.PersonalNumber).Id;
                var shouldSetParentId = !customer.IsRootCustomer && createOrder.Customers.Count > 1;

                var customerOrder = new CustomerOrder
                {
                    CustomerId = customerId,
                    Placed = DateTime.Now,
                    Scheduled = createOrder.ScheduledDateTime,
                    TestType = (int)TestType.Quick,
                    PrefferedLanguage = (int)TestLanguage.Romanian,
                    ParentId = shouldSetParentId ? rootCustomer.Id : null
                };

                ordersToAdd.Add(customerOrder);
            }

            await _context.AddRangeAsync(ordersToAdd);
            await _context.SaveChangesAsync();

            return ordersToAdd;
        }

        public async Task UpdateOrder(UpdateOrderRequest updateOrderRequest)
        {
            var orderEntity = await _context.CustomerOrders.FindAsync(updateOrderRequest.Id);
            if (orderEntity is null)
                throw new ResourceNotFoundException();

            orderEntity.PrefferedLanguage = (short)updateOrderRequest.TestLanguage;
            orderEntity.TestType = (short)updateOrderRequest.TestType;
            orderEntity.Scheduled = updateOrderRequest.ScheduledDateTime;

            _context.CustomerOrders.Update(orderEntity);

            await _context.SaveChangesAsync();
        }

        public async Task<ProcessedOrder> SaveProcessedOrder(int orderId, long numericCode, byte[] barcode)
        {
            var processedOrder = new ProcessedOrder
            {
                CustomerOrderId = orderId,
                ProcessedAt = DateTime.Now,
                Barcode = barcode,
                NumericCode = numericCode
            };

            await _context.ProcessedOrders.AddAsync(processedOrder);
            await _context.SaveChangesAsync();

            return processedOrder;
        }

        public async Task SetTestResult(int orderId, long numericCode, TestResult testResult)
        {
            var processedOrder = await _context.ProcessedOrders.SingleAsync(x => x.CustomerOrderId == orderId && x.NumericCode == numericCode);

            processedOrder.TestResult = (int)testResult;

            _context.ProcessedOrders.Update(processedOrder);

            await _context.SaveChangesAsync();
        }
    }
}
