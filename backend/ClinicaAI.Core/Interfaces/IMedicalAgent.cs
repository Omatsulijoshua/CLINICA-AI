using System.Threading.Tasks;
using ClinicaAI.Core.Models;

namespace ClinicaAI.Core.Interfaces
{
    public interface IMedicalAgent
    {
        string Name { get; }
        Task ExecuteAsync(AgentExecutionContext context);
    }
}
