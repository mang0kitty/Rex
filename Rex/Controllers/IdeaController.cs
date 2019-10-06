using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rex.Models;

namespace Rex.Controllers
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class IdeaController : ControllerBase
    {
        private readonly RexContext _context;

        public IdeaController(RexContext context)
        {
            _context = context;
        }

        // GET: api/v1/Idea
        [HttpGet]
        public async Task<ActionResult<Idea>> GetRandomIdea()
        {
            var user = await GetOrCreateUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            Random rnd = new Random();
            var ideaList = user.DefaultCollection?.Ideas?.ToList();
            if (!(ideaList?.Any() ?? false))
            {
                return NotFound();
            }

            return ideaList[rnd.Next(ideaList.Count)];
        }

        // GET: api/v1/Idea/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Idea>> GetIdea(string id)
        {
            var user = await GetOrCreateUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var idea = user.DefaultCollection.Ideas.FirstOrDefault(i => i.Id == id);

            if (idea == null)
            {
                return NotFound();
            }

            return idea;
        }

        // PUT: api/v1/Idea/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIdea(string id, Idea idea)
        {
            if (id != idea.Id)
            {
                return BadRequest();
            }

            _context.Entry(idea).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict();
            }

            return NoContent();
        }

        // POST: api/v1/Idea
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Idea>> PostIdea(Idea idea)
        {
            var user = await GetOrCreateUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            idea.Id = Guid.NewGuid().ToString("N");

            user.DefaultCollection.Ideas.Add(idea);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetIdea), new { id = idea.Id }, idea);
        }

        // DELETE: api/v1/Idea/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Idea>> DeleteIdea(string id)
        {
            var user = await GetOrCreateUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var idea = user.DefaultCollection.Ideas.FirstOrDefault(i => i.Id == id);
            if (idea == null)
            {
                return NotFound();
            }

            user.DefaultCollection.Ideas.Remove(idea);
            await _context.SaveChangesAsync();

            return idea;
        }

        private async Task<User> GetOrCreateUserAsync()
        {
            if (!(User.Identity?.IsAuthenticated ?? false))
            {
                return null;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.PrincipalId == User.Identity.Name);
            if (user == null)
            {
                // TODO: Create the user and their default collection in the DB
                user = new User
                {
                    PrincipalId = User.Identity.Name,
                    Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "me@aideen.dev",
                    DefaultCollection = new Collection()
                    {
                        Id = User.Identity.Name,
                        Name = User.Identity.Name,
                        Ideas = new List<Idea>(),
                        RoleAssignments = new List<RoleAssignment>(),
                    },
                };

                user.DefaultCollection.RoleAssignments.Add(new RoleAssignment
                {
                    CollectionId = user.DefaultCollection.Id,
                    UserId = user.PrincipalId,
                    Role = RoleType.Owner
                });

                var userAdd = await _context.Users.AddAsync(user);

                await _context.SaveChangesAsync();

                return userAdd.Entity;
            }

            return user;
        }
    }
}
