using Microsoft.EntityFrameworkCore;
using SMBS_SERVER.Data;
using SMBS_SERVER.Models.Masters;

namespace SMBS_SERVER.Repositories
{
    public class PendingUserRepository
    {
        private readonly ApplicationDbContext _context;

        public PendingUserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Save(PendingUserRegistration user)
        {
            user.CreatedAt = DateTime.UtcNow;
            _context.PendingUserRegistrations.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<PendingUserRegistration?> GetByEmail(string email)
        {
            return await _context.PendingUserRegistrations
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task Delete(PendingUserRegistration user)
        {
            _context.PendingUserRegistrations.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
