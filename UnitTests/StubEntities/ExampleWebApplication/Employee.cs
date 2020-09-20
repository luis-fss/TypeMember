using System;
using System.Collections.Generic;

namespace UnitTests.StubEntities.ExampleWebApplication
{
    public sealed class Employee
    {
        public Employee()
        {
            Employees = new List<Employee>();
            EmployeeTerritories = new List<EmployeeTerritory>();
            Orders = new List<Order>();
        }

        public int EmployeeId { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string Title { get; set; }

        public string TitleOfCourtesy { get; set; }

        public DateTime? BirthDate { get; set; }

        public DateTime? HireDate { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string Region { get; set; }

        public string PostalCode { get; set; }

        public string Country { get; set; }

        public string HomePhone { get; set; }

        public string Extension { get; set; }

        public byte[] Photo { get; set; }

        public string Notes { get; set; }

        public string PhotoPath { get; set; }

        public Employee ReportsTo { get; set; }

        public IList<Employee> Employees { get; set; }

        public IList<EmployeeTerritory> EmployeeTerritories { get; set; }

        public IList<Order> Orders { get; set; }
    }
}
