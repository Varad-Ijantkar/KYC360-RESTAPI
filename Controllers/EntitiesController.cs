using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KYC360RESTAPI.Model;
using Polly;
using Polly.Retry;
using Microsoft.Extensions.Logging;

namespace KYC360RESTAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntitiesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EntitiesController> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        public EntitiesController(AppDbContext context, ILogger<EntitiesController> logger)
        {
            _context = context;
            _logger = logger;
            // Define a retry policy with exponential backoff
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    3, // Retry 3 times
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Retry {retryCount} for {context.PolicyKey} due to {exception}. Waiting {timeSpan} before next retry.");
                    });
        }

        // GET: api/Entities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Entity>>> GetEntities(
            [FromQuery] string? search,
            [FromQuery] string? gender,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string[]? countries,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "Id",
            [FromQuery] bool sortAscending = true)
        {
            var query = _context.Entities.AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.Names.Any(n => (n.FirstName + " " + n.MiddleName + " " + n.Surname).Contains(search)) ||
                                         e.Addresses.Any(a => a.AddressLine.Contains(search) || a.Country.Contains(search)));
            }

            // Gender filter
            if (!string.IsNullOrWhiteSpace(gender))
            {
                query = query.Where(e => e.Gender == gender);
            }

            // Date range filter
            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(e => e.Dates.Any(d => d.Dates >= startDate.Value && d.Dates <= endDate.Value));
            }

            // Country filter
            if (countries != null && countries.Length > 0)
            {
                query = query.Where(e => e.Addresses.Any(a => countries.Contains(a.Country)));
            }

            // Sorting
            switch (sortBy.ToLower())
            {
                case "gender":
                    query = sortAscending ? query.OrderBy(e => e.Gender) : query.OrderByDescending(e => e.Gender);
                    break;
                case "surname":
                    query = sortAscending ? query.OrderBy(e => e.Names.FirstOrDefault().Surname) : query.OrderByDescending(e => e.Names.FirstOrDefault().Surname);
                    break;
                case "country":
                    query = sortAscending ? query.OrderBy(e => e.Addresses.FirstOrDefault().Country) : query.OrderByDescending(e => e.Addresses.FirstOrDefault().Country);
                    break;
                case "firstname":
                    query = sortAscending ? query.OrderBy(e => e.Names.FirstOrDefault().FirstName) : query.OrderByDescending(e => e.Names.FirstOrDefault().FirstName);
                    break;
                default:
                    query = sortAscending ? query.OrderBy(e => e.Id) : query.OrderByDescending(e => e.Id);
                    break;
            }

            // Pagination
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            return await query.ToListAsync();
        }

        // GET: api/Entities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Entity>> GetEntity(string id)
        {
            var entity = await _context.Entities
                .Include(e => e.Addresses)
                .Include(e => e.Dates)
                .Include(e => e.Names)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entity == null)
            {
                return NotFound();
            }

            return entity;
        }

        // POST: api/Entities
        [HttpPost]
        public async Task<ActionResult<Entity>> PostEntity(Entity entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // The Id will be generated by the database, so we ensure it's not set before adding the entity.
            entity.Id = null;

            _context.Entities.Add(entity);
            // Use the retry policy when saving changes
            var saveSuccessful = await SaveChangesWithRetryAsync();
            if (!saveSuccessful)
            {
                return StatusCode(500, "An error occurred while saving the entity.");
            }

            return CreatedAtAction("GetEntity", new { id = entity.Id }, entity);
        }

        // PUT: api/Entities/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEntity(string id, Entity entity)
        {
            if (id != entity.Id)
            {
                return BadRequest();
            }

            _context.Entry(entity).State = EntityState.Modified;

            // Use the retry policy when saving changes
            var saveSuccessful = await SaveChangesWithRetryAsync();
            if (!saveSuccessful)
            {
                if (!EntityExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return StatusCode(500, "An error occurred while updating the entity.");
                }
            }

            return NoContent();
        }

        // DELETE: api/Entities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntity(string id)
        {
            var entity = await _context.Entities.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            _context.Entities.Remove(entity);
            // Use the retry policy when saving changes
            var saveSuccessful = await SaveChangesWithRetryAsync();
            if (!saveSuccessful)
            {
                return StatusCode(500, "An error occurred while deleting the entity.");
            }

            return NoContent();
        }

        private async Task<bool> SaveChangesWithRetryAsync()
        {
            var result = await _retryPolicy.ExecuteAndCaptureAsync(async () =>
            {
                await _context.SaveChangesAsync();
                return true;
            });

            if (result.Outcome == OutcomeType.Failure)
            {
                _logger.LogError($"Failed to save changes after {result.FinalHandledResult} retries.");
                return false;
            }

            return true;
        }

        private bool EntityExists(string id)
        {
            return _context.Entities.Any(e => e.Id == id);
        }
    }
}
