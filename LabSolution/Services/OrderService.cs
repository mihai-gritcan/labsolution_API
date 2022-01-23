using LabSolution.Dtos;
using LabSolution.HttpModels;
using LabSolution.Enums;
using LabSolution.Models;
using LabSolution.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LabSolution.Infrastructure;

namespace LabSolution.Services
{
    public interface IOrderService
    {
        Task<List<CustomerOrder>> SaveOrders(CreateOrderRequest createOrder, IEnumerable<Customer> customersEntities);
        Task<List<DateTime>> GetOccupiedTimeSlots(DateTime date);
        Task<CustomerOrder> GetOrderDetails(int createdOrderId);
        Task UpdateOrder(UpdateOrderRequest updateOrderRequest);
        Task<ProcessedOrder> CreateOrUpdateProcessedOrder(int orderId);
        Task SetTestResult(int processedOrderId, TestResult testResult, string executorName, string verifierName, string validatorName);

        Task<ProcessedOrderForPdf> GetProcessedOrderForPdf(int processedOrderId);

        Task<List<OrderWithStatusResponse>> GetOrdersWithStatus(DateTime date, string idnp, bool includeSyncState);
        Task<List<OrderWithStatusResponse>> GetOrdersWithStatus(DateTime startDate, DateTime endDate, TestType? testType = null);

        Task DeleteOrder(int orderId, bool canRemovePdf);

        Task SaveOrReplacePdfBytes(int processedOrderId, string pdfName, byte[] pdfBytes);
        Task<List<ProcessedOrderToSetResultResponse>> GetOrdersToSetResult(DateTime date, string numericCode);

        Task<ProcessedOrderPdf> GetPdfBytes(int processedOrderId);
        Task<ProcessedOrderPdf> GetPdfBytes(string pdfNameHex);
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

        public Task<List<OrderWithStatusResponse>> GetOrdersWithStatus(DateTime date, string idnp, bool includeSyncState)
        {
            return GetQueryableOrders(idnp: idnp, includeSyncState: includeSyncState)
                .Where(x => x.OrderDate.Date == date)
                .OrderBy(x => x.Status).ThenBy(x => x.OrderId)
                .ToListAsync();
        }

        public Task<List<OrderWithStatusResponse>> GetOrdersWithStatus(DateTime startDate, DateTime endDate, TestType? testType = null)
        {
            return GetQueryableOrders(testType: testType)
                .Where(x => x.OrderDate.Date >= startDate && x.OrderDate.Date <= endDate)
                .OrderBy(x => x.Status).ThenBy(x => x.OrderId)
                .ToListAsync();
        }

        private IQueryable<OrderWithStatusResponse> GetQueryableOrders(string idnp = null, TestType? testType = null, bool includeSyncState = false)
        {
            return _context.CustomerOrders
                .Include(x => x.Customer)
                .Include(x => x.ProcessedOrder).ThenInclude(x => x.OrderSyncToGov)
                .Where(x => (idnp == null || x.Customer.PersonalNumber.Contains(idnp)) && (testType == null || x.TestType == (short)testType))
                .Select(x => new OrderWithStatusResponse
                {
                    OrderId = x.Id,
                    Customer = CustomerDto.CreateDtoFromEntity(x.Customer, x.ParentId == null),
                    TestLanguage = (TestLanguage)x.TestLanguage,
                    TestType = (TestType)x.TestType,
                    ParentId = x.ParentId,
                    OrderDate = x.Scheduled,
                    Status = x.ProcessedOrder == null ? OrderStatus.Created : OrderStatus.Processed,
                    TestResult = x.ProcessedOrder == null ? null : (TestResult)x.ProcessedOrder.TestResult,
                    NumericCode = x.ProcessedOrder == null ? null : x.ProcessedOrder.Id.ToString("D7"),
                    ProcessedOrderId = x.ProcessedOrder == null ? null : x.ProcessedOrder.Id,
                    IsSyncToGov = includeSyncState && x.ProcessedOrder.OrderSyncToGov != null
                })
                .OrderBy(x => x.Status).ThenBy(x => x.OrderId);
        }

        public Task<ProcessedOrderForPdf> GetProcessedOrderForPdf(int processedOrderId)
        {
            return _context.ProcessedOrders
                .Include(x => x.CustomerOrder).ThenInclude(x => x.Customer)
                .Where(x => x.Id == processedOrderId)
                .Select(x => new ProcessedOrderForPdf
                {
                    OrderId = x.Id,
                    TestResult = (TestResult)x.TestResult,
                    OrderDate = x.CustomerOrder.Scheduled,
                    ProcessedAt = x.ProcessedAt,
                    ProcessedBy = x.ProcessedBy,
                    TestType = (TestType)x.CustomerOrder.TestType,
                    TestLanguage = (TestLanguage)x.CustomerOrder.TestLanguage,
                    Customer = CustomerDto.CreateDtoFromEntity(x.CustomerOrder.Customer, x.CustomerOrder.ParentId == null),
                    NumericCode = x.Id.ToString("D7"),
                    PdfName = x.PdfName
                }).SingleAsync();
        }

        public Task<CustomerOrder> GetOrderDetails(int createdOrderId)
        {
            return _context.CustomerOrders.Include(x => x.Customer).SingleOrDefaultAsync(x => x.Id == createdOrderId);
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
                    PlacedAt = DateTime.UtcNow.ToBucharestTimeZone(),
                    Scheduled = createOrder.ScheduledDateTime,
                    TestType = (short)createOrder.TestType,
                    TestLanguage = (short)createOrder.TestLanguage,
                    ParentId = shouldSetParentId ? rootCustomer.Id : null
                };

                ordersToAdd.Add(customerOrder);
            }

            _context.AddRange(ordersToAdd);
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
            var processedOrder = await _context.ProcessedOrders.Where(x => x.CustomerOrderId == orderId).SingleOrDefaultAsync() ?? new ProcessedOrder
            {
                CustomerOrderId = orderId,
                ProcessedAt = DateTime.UtcNow.ToBucharestTimeZone(),
                PrintCount = 1
            };

            if(processedOrder.Id > 0)
            {
                processedOrder.ProcessedAt = DateTime.UtcNow.ToBucharestTimeZone();
                processedOrder.PrintCount++;
            }
            else
            {
                await _context.ProcessedOrders.AddAsync(processedOrder);
            }

            await _context.SaveChangesAsync();

            return processedOrder;
        }

        // TODO: task#40 - return a ProcessedOrderForPdf object and drop GetProcessedOrderForPdf() method
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

        public async Task SaveOrReplacePdfBytes(int processedOrderId, string pdfName, byte[] pdfBytes)
        {
            var processedOrderPdfEntity = new ProcessedOrderPdf
            {
                DateCreated = DateTime.UtcNow.ToBucharestTimeZone(),
                ProcessedOrderId = processedOrderId,
                PdfBytes = pdfBytes
            };

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                var existingPdf = await _context.ProcessedOrderPdfs.SingleOrDefaultAsync(x => x.ProcessedOrderId == processedOrderId);
                if(existingPdf is not null)
                    _context.ProcessedOrderPdfs.Remove(existingPdf);

                _context.ProcessedOrderPdfs.Add(processedOrderPdfEntity);

                var processedOrder = await _context.ProcessedOrders.SingleAsync(x => x.Id == processedOrderId);
                processedOrder.PdfName = pdfName;
                _context.ProcessedOrders.Update(processedOrder);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
        }

        public Task<List<ProcessedOrderToSetResultResponse>> GetOrdersToSetResult(DateTime date, string numericCode)
        {
            return _context.ProcessedOrders.Where(x => x.ProcessedAt.Date == date.Date && (string.IsNullOrWhiteSpace(numericCode) || x.Id.ToString().Contains(numericCode)))
                .Include(x => x.CustomerOrder).ThenInclude(x => x.Customer)
                .Select(x => new ProcessedOrderToSetResultResponse
                {
                    ProcessedOrderId = x.Id,
                    ProcessedAt = x.ProcessedAt,
                    PersonalNumber = x.CustomerOrder.Customer.PersonalNumber,
                    FirstName = x.CustomerOrder.Customer.FirstName,
                    LastName = x.CustomerOrder.Customer.LastName,
                    DateOfBirth = x.CustomerOrder.Customer.DateOfBirth,
                    TestResult = (TestResult?)x.TestResult
                }).OrderBy(x => x.ProcessedOrderId).ToListAsync();
        }

        public async Task<ProcessedOrderPdf> GetPdfBytes(int processedOrderId)
        {
            var orderPdf = await _context.ProcessedOrderPdfs.SingleOrDefaultAsync(x => x.ProcessedOrderId == processedOrderId);
            if (orderPdf == null)
                throw new ResourceNotFoundException();

            return orderPdf;
        }

        public Task<ProcessedOrderPdf> GetPdfBytes(string pdfNameHex)
        {
            return _context.ProcessedOrderPdfs.Include(x => x.ProcessedOrder).SingleOrDefaultAsync(x => x.ProcessedOrder.PdfName == pdfNameHex);
        }

        public async Task DeleteOrder(int orderId, bool canRemovePdf)
        {
            var processedOrderWithTicketEmitted = await _context.ProcessedOrders.SingleOrDefaultAsync(x => x.CustomerOrderId == orderId);
            if (processedOrderWithTicketEmitted is not null)
            {
                var orderWithGeneratedResult = await _context.ProcessedOrderPdfs.SingleOrDefaultAsync(x => x.ProcessedOrderId == processedOrderWithTicketEmitted.Id);
                var wasSynchedWithGov = await _context.OrdersSyncToGov.AnyAsync(x => x.ProcessedOrderId == processedOrderWithTicketEmitted.Id);
                if(wasSynchedWithGov)
                    throw new CustomException("Cannot remove an Order which was synchronized with Gov platform");

                if (orderWithGeneratedResult is not null)
                {
                    if (canRemovePdf)
                        _context.ProcessedOrderPdfs.Remove(orderWithGeneratedResult);
                    else
                        throw new CustomException("Cannot remove an Order which has a ticket emitted and the Result was generated as PDF. That action will produce corrupt data");
                }
                _context.ProcessedOrders.Remove(processedOrderWithTicketEmitted);
            }

            var orderEntity = await _context.CustomerOrders.FindAsync(orderId);
            if (orderEntity == null)
                throw new ResourceNotFoundException();

            _context.CustomerOrders.Remove(orderEntity);
            await _context.SaveChangesAsync();
        }
    }
}
