using LabSolution.Infrastructure;
using LabSolution.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace LabSolution.Dtos
{
    public class CustomerDto
    {
        private const long MinPersonalNumberValue = 1;
        private const long MaxPersonalNumberValue = long.MaxValue;

        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        public Gender Gender { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [StringLength(250)]
        public string Address { get; set; }

        [StringLength(50)]
        public string Passport { get; set; }

        [Required]
        [Range(MinPersonalNumberValue, MaxPersonalNumberValue)]
        public long PersonalNumber { get; set; }

        [StringLength(50)]
        public string Phone { get; set; }

        [StringLength(80)]
        [EmailAddress]
        public string Email { get; set; }

        public bool IsRootCustomer { get; set; }

        public static CustomerDto CreateDtoFromEntity(Customer entity)
        {
            return new CustomerDto
            {
                Id = entity.Id,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                DateOfBirth = entity.DateOfBirth,
                Gender = (Gender)entity.Gender,
                Address = entity.Address,
                Passport = entity.Passport,
                PersonalNumber = long.Parse(entity.PersonalNumber),
                Phone = entity.Phone,
                Email = entity.Email,
                IsRootCustomer = false
            };
        }

        public static CustomerDto CreateDtoFromEntity(Customer entity, bool isRootCustomer)
        {
            return new CustomerDto
            {
                Id = entity.Id,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                DateOfBirth = entity.DateOfBirth,
                Gender = (Gender)entity.Gender,
                Address = entity.Address,
                Passport = entity.Passport,
                PersonalNumber = long.Parse(entity.PersonalNumber),
                Phone = entity.Phone,
                Email = entity.Email,
                IsRootCustomer = isRootCustomer
            };
        }
    }
}
