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
        Task<ProcessedOrder> CreateOrUpdateProcessedOrder(int orderId);
        Task SetTestResult(int processedOrderId, TestResult testResult, string executorName, string verifierName, string validatorName);
        Task<List<FinishedOrderResponse>> GetFinishedOrders(DateTime date, long? idnp);
        Task<FinishedOrderResponse> GetFinishedOrderForPdf(int processedOrderId);
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
            return _context.CustomerOrders.Where(x => x.Scheduled.Date == date.Date && x.ProcessedOrder == null 
                                                    && (idnp == null || x.Customer.PersonalNumber.Contains(idnp.Value.ToString())))
                .Include(x => x.Customer)
                .Select(x => new CreatedOrdersResponse
                {
                    Id = x.Id,
                    CustomerId = x.CustomerId,
                    Customer = CustomerDto.CreateDtoFromEntity(x.Customer, x.ParentId == null),
                    ParentId = x.ParentId,
                    PlacedAt = x.PlacedAt,
                    Scheduled = x.Scheduled,
                    TestLanguage = (TestLanguage)x.TestLanguage,
                    TestType = (TestType)x.TestType
                })
                .OrderBy(x => x.Scheduled)
                .ToListAsync();
        }

        public Task<List<FinishedOrderResponse>> GetFinishedOrders(DateTime date, long? idnp)
        {
            return _context.ProcessedOrders.Where(x => x.TestResult != null && x.ProcessedAt.Date == date.Date
                                                    && (idnp == null || x.CustomerOrder.Customer.PersonalNumber.Contains(idnp.Value.ToString())))
                .Include(x => x.CustomerOrder).ThenInclude(x => x.Customer)
                .Select(x => new FinishedOrderResponse
                {
                    Id = x.Id,
                    TestResult = (TestResult)x.TestResult,
                    OrderDate = x.CustomerOrder.Scheduled,
                    TestType = (TestType)x.CustomerOrder.TestType,
                    TestLanguage = (TestLanguage)x.CustomerOrder.TestLanguage,
                    Customer = CustomerDto.CreateDtoFromEntity(x.CustomerOrder.Customer, x.CustomerOrder.ParentId == null)
                }).OrderBy(x => x.NumericCode).ToListAsync();
        }

        public Task<FinishedOrderResponse> GetFinishedOrderForPdf(int processedOrderId)
        {
            return _context.ProcessedOrders
                .Include(x => x.CustomerOrder).ThenInclude(x => x.Customer)
                .Where(x => x.Id == processedOrderId)
                .Select(x => new FinishedOrderResponse
                {
                    Id = x.Id,
                    TestResult = (TestResult)x.TestResult,
                    OrderDate = x.CustomerOrder.Scheduled,
                    TestType = (TestType)x.CustomerOrder.TestType,
                    TestLanguage = (TestLanguage)x.CustomerOrder.TestLanguage,
                    Customer = CustomerDto.CreateDtoFromEntity(x.CustomerOrder.Customer, x.CustomerOrder.ParentId == null)
                }).SingleAsync();
        }

        public Task<CustomerOrder> GetOrderDetails(int createdOrderId)
        {
            return _context.CustomerOrders.Include(x => x.Customer).SingleOrDefaultAsync(x => x.Id == createdOrderId);
        }

        public async Task<List<CustomerOrder>> SaveOrders(CreateOrderRequest createOrder, IEnumerable<Customer> customersEntities)
        {
            var ordersToAdd = new List<CustomerOrder>();

            var rootCustomer = customersEntities.Single(x => x.PersonalNumber == createOrder.Customers.First(c => c.IsRootCustomer).PersonalNumber.ToString());

            foreach (var customer in createOrder.Customers)
            {
                var customerId = customersEntities.Single(x => x.PersonalNumber == customer.PersonalNumber.ToString()).Id;
                var shouldSetParentId = !customer.IsRootCustomer && createOrder.Customers.Count > 1;

                var customerOrder = new CustomerOrder
                {
                    CustomerId = customerId,
                    PlacedAt = DateTime.Now,
                    Scheduled = createOrder.ScheduledDateTime,
                    TestType = (int)TestType.Antigen,
                    TestLanguage = (int)TestLanguage.Romanian,
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

            orderEntity.TestLanguage = (short)updateOrderRequest.TestLanguage;
            orderEntity.TestType = (short)updateOrderRequest.TestType;
            orderEntity.Scheduled = updateOrderRequest.ScheduledDateTime;

            _context.CustomerOrders.Update(orderEntity);

            await _context.SaveChangesAsync();
        }

        public async Task<ProcessedOrder> CreateOrUpdateProcessedOrder(int orderId)
        {
            var processedOrder = await _context.ProcessedOrders.FindAsync(orderId) ?? new ProcessedOrder
            {
                CustomerOrderId = orderId,
                ProcessedAt = DateTime.Now,
                PrintCount = 1
            };

            if(processedOrder.Id > 0)
            {
                processedOrder.ProcessedAt = DateTime.Now;
                processedOrder.PrintCount++;
            }
            else
            {
                await _context.ProcessedOrders.AddAsync(processedOrder);
            }

            await _context.SaveChangesAsync();

            return processedOrder;
        }

        public async Task SetTestResult(int processedOrderId, TestResult testResult, string executorName, string verifierName, string validatorName)
        {
            var processedOrder = await _context.ProcessedOrders.SingleAsync(x => x.Id == processedOrderId);

            processedOrder.TestResult = (int)testResult;
            processedOrder.ProcessedBy = executorName;
            processedOrder.CheckedBy = verifierName;
            processedOrder.ValidatedBy = validatorName;

            _context.ProcessedOrders.Update(processedOrder);

            await _context.SaveChangesAsync();
        }
    }
}
