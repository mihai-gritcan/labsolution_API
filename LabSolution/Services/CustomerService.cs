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
            var customersFirstNames = customers.Select(x => x.FirstName);
            var customersLastNames = customers.Select(x => x.LastName);
            var customersDOBs = customers.Select(x => x.DateOfBirth.Date);

            var existingCustomers = await _context.Customers.Where(x =>
                    customersPersonalNumbers.Contains(x.PersonalNumber)
                    || customersFirstNames.Contains(x.FirstName)
                    || customersLastNames.Contains(x.LastName)
                    || customersDOBs.Contains(x.DateOfBirth.Date)
                ).ToListAsync();

            var customersToAdd = new List<Customer>();
            var matchedCustomers = new List<Customer>();

            var customersWithPersonalNumber = customers.Where(x => !string.IsNullOrWhiteSpace(x.PersonalNumber)).ToHashSet();
            var customersWithoutPersonalNumber = customers.Except(customersWithPersonalNumber).ToHashSet();

            foreach (var item in customersWithPersonalNumber)
            {
                var entity = existingCustomers.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.PersonalNumber) 
                    && x.PersonalNumber.Equals(item.PersonalNumber, System.StringComparison.InvariantCultureIgnoreCase));
                
                if (entity is not null)
                {
                    matchedCustomers.Add(entity);
                    continue;
                }
                entity = new Customer
                {
                    PersonalNumber = item.PersonalNumber,
                    Passport = item.Passport,
                    FirstName = item.FirstName,
                    LastName = item.LastName,
                    DateOfBirth = item.DateOfBirth,
                    Email = item.Email,
                    Address = item.Address,
                    Gender = (int)item.Gender,
                    Phone = item.Phone
                };
                customersToAdd.Add(entity);
            }

            foreach (var customer in customersWithoutPersonalNumber)
            {
                var customerEntity = existingCustomers.FirstOrDefault(x =>
                    x.FirstName.Equals(customer.FirstName, System.StringComparison.InvariantCultureIgnoreCase)
                    && x.LastName.Equals(customer.LastName, System.StringComparison.InvariantCultureIgnoreCase)
                    && x.DateOfBirth.Date == customer.DateOfBirth.Date);

                if (customerEntity is not null)
                {
                    matchedCustomers.Add(customerEntity);
                    continue;
                }

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
                _context.Customers.AddRange(customersToAdd);
                await _context.SaveChangesAsync();
            }

            return matchedCustomers.Union(customersToAdd).ToList();
        }
    }
}
