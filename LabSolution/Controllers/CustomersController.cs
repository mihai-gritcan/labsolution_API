using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabSolution.Models;
using LabSolution.Dtos;

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

        [HttpGet]
        public async Task<IActionResult> GetCustomers([FromQuery] long? idnp, [FromQuery] string firstName, [FromQuery] string lastName)
        {
            var queryableCustomers = _context.Customers.Select(x => x);
            if (idnp.HasValue)
                queryableCustomers = queryableCustomers.Where(x => x.PersonalNumber.Contains(idnp.Value.ToString()));

            if(!string.IsNullOrWhiteSpace(firstName))
                queryableCustomers = queryableCustomers.Where(x => x.FirstName.Contains(firstName));

            if (!string.IsNullOrWhiteSpace(lastName))
                queryableCustomers = queryableCustomers.Where(x => x.LastName.Contains(lastName));

            return Ok(await queryableCustomers.Select(x => CustomerDto.CreateDtoFromEntity(x)).ToListAsync());
        }

        // reception update Customer details
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, CustomerDto customer)
        {
            if (id != customer.Id)
            {
                return BadRequest();
            }

            var customerEntity = await _context.Customers.FindAsync(customer.Id);
            if (customerEntity is null)
                return NotFound();

            customerEntity.FirstName = customer.FirstName;
            customerEntity.LastName = customer.LastName;
            customerEntity.Gender = (int)customer.Gender;
            customerEntity.DateOfBirth = customer.DateOfBirth;
            customerEntity.Address = customer.Address;
            customerEntity.Passport = customer.Passport;
            customerEntity.PersonalNumber = customer.PersonalNumber.ToString();
            customerEntity.Phone = customer.Phone;
            customerEntity.Email = customer.Email;

            _context.Customers.Update(customerEntity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }
    }
}
