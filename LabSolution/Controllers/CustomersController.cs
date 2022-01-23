using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabSolution.Models;
using LabSolution.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace LabSolution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : BaseApiController
    {
        private readonly LabSolutionContext _context;

        public CustomersController(LabSolutionContext context)
        {
            _context = context;
        }

        [HttpDelete("{customerId}")]
        public async Task<IActionResult> SoftDeleteCustomer(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer is null)
                return NotFound("Resource not found");

            customer.IsSoftDelete = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomers([FromQuery] string idnp, [FromQuery] string firstName, [FromQuery] string lastName)
        {
            var queryableCustomers = _context.Customers.Where(x => !x.IsSoftDelete).Select(x => x);
            if (!string.IsNullOrWhiteSpace(idnp))
                queryableCustomers = queryableCustomers.Where(x => x.PersonalNumber.Contains(idnp));

            if(!string.IsNullOrWhiteSpace(firstName))
                queryableCustomers = queryableCustomers.Where(x => x.FirstName.Contains(firstName) || x.LastName.Contains(firstName));

            if (!string.IsNullOrWhiteSpace(lastName))
                queryableCustomers = queryableCustomers.Where(x => x.LastName.Contains(lastName) || x.FirstName.Contains(lastName));

            return Ok(await queryableCustomers.Select(x => CustomerDto.CreateDtoFromEntity(x)).ToListAsync());
        }

        // reception update Customer details
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, CustomerDto customer)
        {
            if (id != customer.Id)
                return BadRequest();

            return await UpdateCustomer(id, customer);
        }

        private async Task<IActionResult> UpdateCustomer(int id, CustomerDto customer)
        {
            var customerEntity = await _context.Customers.FindAsync(customer.Id);
            if (customerEntity is null)
                return NotFound();

            customerEntity.FirstName = customer.FirstName;
            customerEntity.LastName = customer.LastName;
            customerEntity.Gender = (int)customer.Gender;
            customerEntity.DateOfBirth = customer.DateOfBirth;
            customerEntity.Address = customer.Address;
            customerEntity.Passport = customer.Passport;
            customerEntity.PersonalNumber = customer.PersonalNumber;
            customerEntity.Phone = customer.Phone;
            customerEntity.Email = customer.Email;

            var existingSimilarByPersonalNumber = await _context.Customers.FirstOrDefaultAsync(x => x.Id != customerEntity.Id &&
                !string.IsNullOrWhiteSpace(customerEntity.PersonalNumber) && x.PersonalNumber.ToUpper().Equals(customerEntity.PersonalNumber.ToUpper()));

            if (existingSimilarByPersonalNumber is not null)
                return BadRequest("There is already a customer registered with this Personal Number");

            var existingSimilarByNameAndDoB = await _context.Customers.FirstOrDefaultAsync(x =>
                x.Id != customerEntity.Id
                && x.FirstName.ToUpper().Equals(customerEntity.FirstName.ToUpper())
                && x.LastName.ToUpper().Equals(customerEntity.LastName.ToUpper())
                && x.DateOfBirth.Date == customerEntity.DateOfBirth.Date);

            if (existingSimilarByNameAndDoB is not null)
                return BadRequest("There is already a customer registered with this Name and Date Of Birth");

            _context.Customers.Update(customerEntity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CustomerExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        private Task<bool> CustomerExists(int id)
        {
            return _context.Customers.AnyAsync(e => e.Id == id);
        }
    }
}
