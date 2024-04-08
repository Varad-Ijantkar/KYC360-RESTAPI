using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KYC360RESTAPI.Model
{
    public interface IEntity
    {
        string Id { get; set; }
        bool Deceased { get; set; }
        string? Gender { get; set; }
        List<Address>? Addresses { get; set; }
        List<Date> Dates { get; set; }
        List<Name> Names { get; set; }
    }

    public class Entity : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public bool Deceased { get; set; }
        public string? Gender { get; set; }
        public List<Address>? Addresses { get; set; } = new List<Address>();
        public List<Date> Dates { get; set; } = new List<Date>();
        public List<Name> Names { get; set; } = new List<Name>();
    }


    public class Address
    {
        public string? AddressLine { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }

    public class Date
    {
        public string? DateType { get; set; }
        public DateTime? Dates { get; set; }
    }

    public class Name
    {
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? Surname { get; set; }
    }
}
