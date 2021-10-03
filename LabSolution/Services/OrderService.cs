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
        Task<List<CreatedOrdersResponse>> GetOrders(DateTime date);
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

        public Task<List<CreatedOrdersResponse>> GetOrders(DateTime date)
        {
            return _context.CustomerOrders.Where(x => x.Scheduled.Date == date.Date)
                .Include(x => x.Customer)
                .Select(x => new CreatedOrdersResponse
                {
                    Id = x.Id,
                    CustomerId = x.CustomerId,
                    Customer = CustomerDto.CreateDtoFromEntity(x.Customer),
                    ParentId = x.ParentId,
                    Placed = x.Placed,
                    Scheduled = x.Scheduled,
                    PrefferedLanguage = (TestLanguages)x.PrefferedLanguage,
                    TestType = (TestTypes)x.TestType
                })
                .ToListAsync();
        }

        public async Task<List<CustomerOrder>> SaveOrders(CreateOrderRequest createOrder, IEnumerable<Customer> customersEntities)
        {
            var ordersToAdd = new List<CustomerOrder>();

            var shouldSetParentId = createOrder.Customers.Count > 1;
            var rootCustomer = customersEntities.Single(x => x.PersonalNumber == createOrder.Customers.First(c => c.IsRootCustomer).PersonalNumber);

            foreach (var customer in createOrder.Customers)
            {
                var customerId = customersEntities.Single(x => x.PersonalNumber == customer.PersonalNumber).Id;

                var customerOrder = new CustomerOrder
                {
                    CustomerId = customerId,
                    Placed = DateTime.Now,
                    Scheduled = createOrder.ScheduledTime,
                    TestType = (int)TestTypes.Quick,
                    PrefferedLanguage = (int)TestLanguages.Romanian,
                    ParentId = shouldSetParentId ? rootCustomer.Id : null
                };

                ordersToAdd.Add(customerOrder);
            }

            await _context.AddRangeAsync(ordersToAdd);
            await _context.SaveChangesAsync();

            return ordersToAdd;
        }
    }
}
