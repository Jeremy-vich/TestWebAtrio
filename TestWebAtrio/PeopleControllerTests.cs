using Microsoft.AspNetCore.Mvc;
using WebAtrio.Controllers;

namespace TestWebAtrio
{
    using System;
    using WebAtrio.Contexts;
    using WebAtrio.Models;
    using Xunit;

    public class JobTests : IDisposable
    {
        private readonly CandidatesContext _context;
        private static JobTests? instance;
        public static JobTests Instance => CreateOrReuseInstance();

        private static JobTests CreateOrReuseInstance()
        {
            if (instance != null) return instance;
            var semaphore = new SemaphoreSlim(1, 1);
            instance ??= new JobTests();
            semaphore.Release(1);

            return instance;
        }

        public JobTests()
        {
            var databaseName = Guid.NewGuid().ToString();
            _context = new CandidatesContext(databaseName);
            SeedData();
        }

        private void SeedData()
        {
            var people = new List<Person>
            {
                new Person { Id = 1, Name = "John", LastName = "Doe", BirthDate = new DateTime(1980, 1, 1) },
                new Person { Id = 2, Name = "Jane", LastName = "Smith", BirthDate = new DateTime(1990, 5, 5) },
                new Person { Id = 3, Name = "Gabriel", LastName = "Martin", BirthDate = new DateTime(1980, 1, 1) },
            };

            _context.People.AddRange(people);

            var jobs = new List<Job>
            {
                new Job { Id = 1, JobName = "Developer", CompanyName = "ABC Inc.", StartDate = new DateTime(2022, 1, 1), EndDate = new DateTime(2022, 12, 31), PersonId = 1 },
                new Job { Id = 2, JobName = "Manager", CompanyName = "XYZ Corp.", StartDate = new DateTime(2021, 1, 1), PersonId = 2 },
                new Job { Id = 3, JobName = "Manager", CompanyName = "ABC Inc.", StartDate = new DateTime(2022, 1, 1), EndDate = new DateTime(2022, 12, 31), PersonId = 3 },
                new Job { Id = 4, JobName = "Tester", CompanyName = "Test Inc.", StartDate = new DateTime(2022, 1, 1), EndDate = new DateTime(2022, 6, 30), PersonId = 2 }
            };

            _context.Jobs.AddRange(jobs);

            _context.SaveChanges();
        }

        [Fact]
        public async void GetPeople_ReturnsAllPeopleOrderedByName()
        {
            var controller = new PeopleController(_context);

            var result = await controller.GetPeople();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var people = Assert.IsAssignableFrom<IEnumerable<Person>>(okResult.Value);
            Assert.Equal(3, people.Count());

            var firstPerson = people.First();
            var lastPerson = people.Last();

            Assert.Equal("Gabriel", firstPerson.Name);
            Assert.Equal("John", lastPerson.Name);
        }

        [Fact]
        public async void AddPerson_WithPersonUnder150Years_ReturnsCreatedAtAction()
        {
            var controller = new PeopleController(_context);
            var newPerson = new Person { Name = "New", LastName = "Person", BirthDate = DateTime.Now.AddYears(-30) };

            var result = await controller.PostPerson(newPerson);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetPerson", createdAtActionResult.ActionName);
        }

        [Fact]
        public async void AddPerson_WithPersonOver150Years_ReturnsBadRequest()
        {
            var controller = new PeopleController(_context);
            var newPerson = new Person { Name = "Old", LastName = "Person", BirthDate = DateTime.Now.AddYears(-160) };

            var result = await controller.PostPerson(newPerson);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async void AddJob_WithValidPersonId_ReturnsCreatedAtAction()
        {
            var controller = new PeopleController(_context);
            var newJob = new Job { JobName = "Tester", StartDate = DateTime.Now.AddMonths(-1), EndDate = DateTime.Now.AddMonths(1) };

            var result = await controller.AddJob(1, newJob);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetPeople", createdAtActionResult.ActionName);
        }

        [Fact]
        public async void AddJob_WithInvalidPersonId_ReturnsNotFound()
        {
            var controller = new PeopleController(_context);
            var newJob = new Job { JobName = "Tester", StartDate = DateTime.Now.AddMonths(-1), EndDate = DateTime.Now.AddMonths(1) };

            var result = await controller.AddJob(0, newJob);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async void GetPeopleByCompany_WithValidCompany_ReturnsPeopleForCompany()
        {
            var controller = new PeopleController(_context);
            var companyName = "ABC Inc.";

            var result = await controller.GetPeopleByCompany(companyName);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var people = Assert.IsAssignableFrom<IEnumerable<Person>>(okResult.Value);
            Assert.Equal(2, people.Count());

            var personNames = people.Select(p => p.Name).ToList();
            Assert.Contains("John", personNames);
        }

        [Fact]
        public async void GetPeopleByCompany_WithInvalidCompany_ReturnsNotFound()
        {
            var controller = new PeopleController(_context);
            var companyName = "Nonexistent Corp.";

            var result = await controller.GetPeopleByCompany(companyName);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async void GetPeopleByCompany_WithNoPeopleForCompany_ReturnsEmptyList()
        {
            var controller = new PeopleController(_context);
            var companyName = "XYZ Corp.";

            var result = await controller.GetPeopleByCompany(companyName);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var people = Assert.IsAssignableFrom<IEnumerable<Person>>(okResult.Value);
            Assert.Single(people);

            var personNames = people.Select(p => p.Name).ToList();
            Assert.Contains("Jane", personNames);
        }

        [Fact]
        public async void GetJobsBetweenDates_WithValidDates_ReturnsJobsInDateRange()
        {
            var controller = new PeopleController(_context);
            var startDate = new DateTime(2022, 1, 1);
            var endDate = new DateTime(2022, 12, 31);

            var result = await controller.GetJobsBetweenDates(personId: 1, startDate, endDate);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var jobs = Assert.IsAssignableFrom<IEnumerable<Job>>(okResult.Value);
            Assert.Single(jobs);

            var jobNames = jobs.Select(j => j.JobName).ToList();
            Assert.Contains("Developer", jobNames);
        }

        [Fact]
        public async void GetJobsBetweenDates_WithNoJobsInDateRange_ReturnsEmptyList()
        {
            var controller = new PeopleController(_context);
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 12, 31);

            var result = await controller.GetJobsBetweenDates(personId: 1, startDate, endDate);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var jobs = Assert.IsAssignableFrom<IEnumerable<Job>>(okResult.Value);
            Assert.Empty(jobs);
        }

        [Fact]
        public async void GetJobsBetweenDates_WithJobsForMultiplePeopleInDateRange_ReturnsJobsForSpecificPerson()
        {
            var controller = new PeopleController(_context);
            var startDate = new DateTime(2021, 1, 1);
            var endDate = new DateTime(2023, 12, 31);

            var result = await controller.GetJobsBetweenDates(personId: 2, startDate, endDate);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var jobs = Assert.IsAssignableFrom<IEnumerable<Job>>(okResult.Value);
            Assert.Equal(2, jobs.Count());

            var jobNames = jobs.Select(j => j.JobName).ToList();
            Assert.Contains("Manager", jobNames);
            Assert.Contains("Tester", jobNames);
        }

        [Fact]
        public async void GetJobsBetweenDates_WithInvalidPersonId_ReturnsNotFound()
        {
            var controller = new PeopleController(_context);
            var startDate = new DateTime(2022, 1, 1);
            var endDate = new DateTime(2022, 12, 31);

            var result = await controller.GetJobsBetweenDates(personId: 999, startDate, endDate);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}