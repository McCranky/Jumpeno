using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Jumpeno.Data {
    /**
     * Zabezpečuje prácu s databázou s využitím nami upravenej identity
     */
    public class JumpenoDbContext : IdentityDbContext<JumpenoUser> {
        public JumpenoDbContext(DbContextOptions<JumpenoDbContext> options) : base(options) { 
        }
    }
}
