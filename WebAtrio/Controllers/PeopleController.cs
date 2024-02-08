using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WebAtrio.Contexts;
using WebAtrio.Models;

namespace WebAtrio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeopleController : ControllerBase
    {
        private readonly CandidatesContext _context;

        public PeopleController(CandidatesContext context)
        {
            _context = context;
        }

        // GET: api/Persones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Person>>> GetPeople()
        {
            var peaople = await _context.People.Include(p => p.Jobs).OrderBy(x => x.Name).ToListAsync();
            return Ok(peaople);
        }

        // POST: api/Persones
        [HttpPost]
        [SwaggerOperation(Description = "Creates a new person with a date of birth not exceeding 150 years.")]
        public async Task<ActionResult<Person>> PostPerson(Person person)
        {
            // Only persons under 150 years of age can be registered
            if (person.Age > 150)
            {
                return BadRequest("Person is too old");
            }

            _context.People.Add(person);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: api/People/{personId}/Jobs
        [HttpPost("{personId}/Jobs")]
        public async Task<ActionResult<Job>> AddJob(int personId, [FromBody] Job job)
        {
            var person = await _context.People.FindAsync(personId);

            if (person == null)
            {
                return NotFound();
            }

            person.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // GET: api/People/WorkedForCompany?companyName={companyName}
        [HttpGet("WorkedForCompany")]
        public async Task<ActionResult<IEnumerable<Person>>> GetPeopleByCompany([FromQuery] string companyName)
        {
            var people = await _context.People
                .Where(p => p.Jobs.Any(job => job.CompanyName == companyName))
                .OrderBy(p => p.Name)
                .Include(p => p.Jobs)
            .ToListAsync();

            if(people?.Count == 0)
            {
                return NotFound();
            }

            return Ok(people);
        }

        // GET: api/People/{personId}/JobsBetweenDates?startDate={startDate}&endDate={endDate}
        [HttpGet("{personId}/JobsBetweenDates")]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobsBetweenDates(int personId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var person = await _context.People.FindAsync(personId);

            if (person == null)
            {
                return NotFound();
            }

            var jobs = _context.Entry(person)
                .Collection(p => p.Jobs)
                .Query()
                .Where(job => job.StartDate >= startDate && job.EndDate <= endDate)
                .ToList();

            return Ok(jobs);
        }
    }
}
