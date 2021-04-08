using System.Text.Json;
using System.Threading.Tasks;

namespace DiscordRollerBot
{
    public interface IDiscordInterface
    {
        Task<bool> Start();
        Task<bool> Stop();

        DiscordInterfaceStatus State {get; }
    }
}
