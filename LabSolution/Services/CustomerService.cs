using LabSolution.Dtos;
using LabSolution.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LabSolution.Services
{
    public interface ICustomerService
    {
        Task<List<Customer>> SaveCustomers(List<CustomerDto> customers);
    }

    public class CustomerService : ICustomerService
    {
        private readonly LabSolutionContext _context;

        public CustomerService(LabSolutionContext context)
        {
            _context = context;
        }

        public async Task<List<Customer>> SaveCustomers(List<CustomerDto> customers)
        {
            var customersPersonalNumbers = customers.Select(x => x.PersonalNumber);

            var existingCustomers = await _context.Customers.Where(x => customersPersonalNumbers.Contains(x.PersonalNumber)).ToListAsync();
            var customersToAdd = new List<Customer>();

            foreach (var customer in customers)
            {
                var customerEntity = existingCustomers.SingleOrDefault(x => x.PersonalNumber == customer.PersonalNumber);
                if (customerEntity is not null)
                    continue;

                customerEntity = new Customer
                {
                    PersonalNumber = customer.PersonalNumber,
                    Passport = customer.Passport,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    DateOfBirth = customer.DateOfBirth,
                    Email = customer.Email,
                    Address = customer.Address,
                    Gender = (int)customer.Gender,
                    Phone = customer.Phone
                };
                customersToAdd.Add(customerEntity);
            }

            if (customersToAdd.Count > 0)
            {
                await _context.Customers.AddRangeAsync(customersToAdd);
                await _context.SaveChangesAsync();
            }

            return existingCustomers.Union(customersToAdd).ToList();
        }
    }
}
