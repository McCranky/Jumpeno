using Microsoft.AspNetCore.Identity;

namespace Jumpeno.Data {
    /**
     * Reprezentuje identitu hráča, ktorú sme si upravili od pôvodnej Identity
     */
    public class JumpenoUser : IdentityUser {
        public int GamesPlayed { get; set; }
        public int GameTimeTotal { get; set; }
        public string Skin { get; set; }
    }
}
