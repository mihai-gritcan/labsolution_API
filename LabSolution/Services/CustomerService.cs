using LabSolution.Dtos;
using LabSolution.Infrastructure;
using LabSolution.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;

namespace LabSolution.Services
{
    public interface ICustomerService
    {
        Task<List<Customer>> CreateCustomers(List<CustomerDto> customers);
    }

    public class CustomerService : ICustomerService
    {
        private readonly LabSolutionContext _context;
        private readonly IConfiguration _appConfiguration;

        public CustomerService(LabSolutionContext context, IConfiguration appConfiguration)
        {
            _context = context;
            _appConfiguration = appConfiguration;
        }

        public async Task<List<Customer>> CreateCustomers(List<CustomerDto> customers)
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

            var denyMatchByPersonalNumberWithDifferentName = 
                _appConfiguration["CustomerDenyMatchByPersonalNumberWithDifferentName"].Equals("true", StringComparison.InvariantCultureIgnoreCase);

            foreach (var cust in customersWithPersonalNumber)
            {
                CheckMatchByPersonalNumberWithDifferentName(existingCustomers, cust, denyMatchByPersonalNumberWithDifferentName);

                var entity = existingCustomers.Find(x => !string.IsNullOrWhiteSpace(x.PersonalNumber)
                    && x.PersonalNumber.Equals(cust.PersonalNumber, StringComparison.InvariantCultureIgnoreCase));

                if (entity is not null)
                {
                    matchedCustomers.Add(entity);
                    continue;
                }
                entity = new Customer
                {
                    PersonalNumber = cust.PersonalNumber,
                    Passport = cust.Passport,
                    FirstName = cust.FirstName,
                    LastName = cust.LastName,
                    DateOfBirth = cust.DateOfBirth,
                    Email = cust.Email,
                    Address = cust.Address,
                    Gender = (int)cust.Gender,
                    Phone = cust.Phone
                };
                customersToAdd.Add(entity);
            }

            foreach (var customer in customersWithoutPersonalNumber)
            {
                var customerEntity = existingCustomers.Find(x =>
                    x.FirstName.Equals(customer.FirstName, StringComparison.InvariantCultureIgnoreCase)
                    && x.LastName.Equals(customer.LastName, StringComparison.InvariantCultureIgnoreCase)
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

        private static void CheckMatchByPersonalNumberWithDifferentName(List<Customer> existingCustomers, CustomerDto customerToUpdate, bool denyMatchByPersonalNumberWithDifferentName)
        {
            if (denyMatchByPersonalNumberWithDifferentName)
            {
                var isAnyWithSamePersonalNumberButDifferentName =
                    !string.IsNullOrWhiteSpace(customerToUpdate.PersonalNumber)
                    && existingCustomers.Any(x => x.PersonalNumber != null
                                                && x.PersonalNumber.Equals(customerToUpdate.PersonalNumber, StringComparison.InvariantCultureIgnoreCase)
                                                && !x.FirstName.Equals(customerToUpdate.FirstName, StringComparison.InvariantCultureIgnoreCase)
                                                && !x.LastName.Equals(customerToUpdate.LastName, StringComparison.InvariantCultureIgnoreCase));

                if (isAnyWithSamePersonalNumberButDifferentName)
                    throw new CustomException($"There is already someone with the Personal Number {customerToUpdate.PersonalNumber}, but with a different Name.");
            }
        }
    }
}
