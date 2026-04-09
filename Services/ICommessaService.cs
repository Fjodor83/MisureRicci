using MisureRicci.Models;
using MisureRicci.Models.ViewModels;

namespace MisureRicci.Services
{
    public interface ICommessaService :
        ICommessaQueryService,
        ICommessaCommandService,
        ICommessaMisuraLinkService
    {
    }
}
