using Blazored.SessionStorage;
using System.Threading.Tasks;

namespace Jumpeno.Services {
    public class LocalStorageTrackingService {
        public enum Item {
            GAME_CODE,
            PLAYER_LOGIN_METHOD,
            PLAYER_NAME,
            PLAYER_SKIN,
            PLAYER_KEY
        }

        public enum LogInMethod {
            SPECTATOR,
            ANONYM,
            IDENTITY
        }

        private readonly ISessionStorageService _SessionStorage;

        public LocalStorageTrackingService(ISessionStorageService sessionStorage) {
            _SessionStorage = sessionStorage;
        }

        public async Task<string> GetItemValue(Item itemType) {
            return await _SessionStorage.GetItemAsync<string>(itemType.ToString());
        }

        public async Task SetItemValue(Item itemType, string value) {
            await _SessionStorage.SetItemAsync(itemType.ToString(), value);
        }

        public async Task RemoveItemValue(Item itemType) {
            await _SessionStorage.RemoveItemAsync(itemType.ToString());
        }
    }
}
