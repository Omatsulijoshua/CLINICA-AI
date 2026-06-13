using System.Linq;
using System.Threading.Tasks;
using Xunit;
using ClinicaAI.Core.Models;
using ClinicaAI.Infrastructure.Agents;

namespace ClinicaAI.Tests
{
    public class LabAgentTests
    {
        [Fact]
        public async Task ExecuteAsync_LowHemoglobin_FlagsAnemia()
        {
            // Arrange
            var agent = new LabAgent();
            var context = new AgentExecutionContext
            {
                ExtractedReportText = "Patient Blood panel results:\nHemoglobin is 9.2 g/dL\nWBC is 6.5\n",
                Country = "USA"
            };

            // Act
            await agent.ExecuteAsync(context);

            // Assert
            Assert.Contains(context.Response.PossibleCauses, c => c.Name.Contains("Low Hemoglobin"));
            Assert.Equal("Medium", context.Response.RiskLevel);
        }

        [Fact]
        public async Task ExecuteAsync_ElevatedGlucose_FlagsDiabetes()
        {
            // Arrange
            var agent = new LabAgent();
            var context = new AgentExecutionContext
            {
                UserInput = "My blood sugar fasting glucose value came back as 135 mg/dL",
                Country = "USA"
            };

            // Act
            await agent.ExecuteAsync(context);

            // Assert
            Assert.Contains(context.Response.PossibleCauses, c => c.Name.Contains("Elevated Fasting Glucose"));
        }

        [Fact]
        public async Task ExecuteAsync_HighTSH_FlagsHypothyroidism()
        {
            // Arrange
            var agent = new LabAgent();
            var context = new AgentExecutionContext
            {
                UserInput = "TSH: 5.6 mIU/L",
                Country = "USA"
            };

            // Act
            await agent.ExecuteAsync(context);

            // Assert
            Assert.Contains(context.Response.PossibleCauses, c => c.Name.Contains("Elevated TSH"));
        }
    }
}
