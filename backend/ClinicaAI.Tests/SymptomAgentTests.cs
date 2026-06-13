using System.Linq;
using System.Threading.Tasks;
using Xunit;
using ClinicaAI.Core.Models;
using ClinicaAI.Infrastructure.Agents;

namespace ClinicaAI.Tests
{
    public class SymptomAgentTests
    {
        [Fact]
        public async Task ExecuteAsync_NigeriaFever_SuggestsMalariaAndTyphoid()
        {
            // Arrange
            var agent = new SymptomAgent();
            var context = new AgentExecutionContext
            {
                UserInput = "I have a high fever and chills.",
                Country = "Nigeria"
            };

            // Act
            await agent.ExecuteAsync(context);

            // Assert
            Assert.Contains(context.Response.PossibleCauses, c => c.Name == "Malaria");
            Assert.Contains(context.Response.PossibleCauses, c => c.Name == "Typhoid Fever");
            Assert.Equal("Medium", context.Response.RiskLevel);
        }

        [Fact]
        public async Task ExecuteAsync_USATickBite_SuggestsLymeDisease()
        {
            // Arrange
            var agent = new SymptomAgent();
            var context = new AgentExecutionContext
            {
                UserInput = "I was in the woods and got a tick bite and now have a circular rash",
                Country = "USA",
                StateRegion = "New York"
            };

            // Act
            await agent.ExecuteAsync(context);

            // Assert
            Assert.Contains(context.Response.PossibleCauses, c => c.Name == "Lyme Disease");
        }

        [Fact]
        public async Task ExecuteAsync_UKFeverWinter_SuggestsInfluenzaAndCovid()
        {
            // Arrange
            var agent = new SymptomAgent();
            var context = new AgentExecutionContext
            {
                UserInput = "High fever, body aches and a dry cough.",
                Country = "UK"
            };

            // Act
            await agent.ExecuteAsync(context);

            // Assert
            Assert.Contains(context.Response.PossibleCauses, c => c.Name == "Influenza (Flu)");
            Assert.Contains(context.Response.PossibleCauses, c => c.Name == "COVID-19");
        }
    }
}
