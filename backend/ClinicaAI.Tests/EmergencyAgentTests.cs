using System.Threading.Tasks;
using Xunit;
using ClinicaAI.Core.Models;
using ClinicaAI.Infrastructure.Agents;

namespace ClinicaAI.Tests
{
    public class EmergencyAgentTests
    {
        [Fact]
        public async Task ExecuteAsync_ChestPain_TriggersEmergencyMode()
        {
            // Arrange
            var agent = new EmergencyAgent();
            var context = new AgentExecutionContext
            {
                UserInput = "I am having severe chest pain radiating down my left arm and jaw"
            };

            // Act
            await agent.ExecuteAsync(context);

            // Assert
            Assert.True(context.Response.IsEmergency);
            Assert.Equal("Critical / Emergency", context.Response.RiskLevel);
            Assert.Contains("Call your local emergency number", context.Response.EmergencyGuidance);
        }

        [Fact]
        public async Task ExecuteAsync_StrokeDroopingFace_TriggersEmergencyMode()
        {
            // Arrange
            var agent = new EmergencyAgent();
            var context = new AgentExecutionContext
            {
                UserInput = "My grandmother has sudden face drooping and slurred speech"
            };

            // Act
            await agent.ExecuteAsync(context);

            // Assert
            Assert.True(context.Response.IsEmergency);
            Assert.Contains("Think FAST", context.Response.EmergencyGuidance);
        }
    }
}
