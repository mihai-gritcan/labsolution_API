using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, CustomerDto customer)
        {
            if (id != customer.Id)
            {
                return BadRequest();
            }

            var entity = CustomerDto.CreateEntityFromDto(customer);

            _context.Entry(entity).State = EntityState.Modified;

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

        #region Unnecessary methods

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await _context.Customers.ToListAsync();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        public async Task<ActionResult<Customer>> PostCustomer(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        #endregion Unnecessary methods
    }
}
