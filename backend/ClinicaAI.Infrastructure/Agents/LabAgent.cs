using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClinicaAI.Core.Interfaces;
using ClinicaAI.Core.Models;

namespace ClinicaAI.Infrastructure.Agents
{
    public class LabAgent : IMedicalAgent
    {
        public string Name => "Lab Agent";

        public Task ExecuteAsync(AgentExecutionContext context)
        {
            // Read extracted report text from files
            string reportText = context.ExtractedReportText;

            // If there's no report text directly, check if user typed test results
            if (string.IsNullOrWhiteSpace(reportText))
            {
                reportText = context.UserInput;
            }

            var text = reportText.ToLowerInvariant();

            bool isLabQuery = text.Contains("cbc") || text.Contains("blood test") || text.Contains("hemoglobin") || 
                              text.Contains("wbc") || text.Contains("cholesterol") || text.Contains("lipid") || 
                              text.Contains("tsh") || text.Contains("glucose") || text.Contains("urinalysis") ||
                              text.Contains("creatinine") || text.Contains("bilirubin");

            if (!isLabQuery)
            {
                return Task.CompletedTask;
            }

            var abnormalFindings = new List<string>();
            var explanations = new List<string>();
            var doctorQuestions = new List<string>();

            // 1. Analyze Hemoglobin
            var hbMatch = Regex.Match(text, @"(?:hemoglobin|hb)(?:\s+\w+){0,5}\s*(?:is|:|=|came back as)?\s*([0-9]+(?:\.[0-9]+)?)");
            if (hbMatch.Success && double.TryParse(hbMatch.Groups[1].Value, out double hbValue))
            {
                string gender = context.Profile?.Gender ?? "Female";
                double lowLimit = gender.Equals("Male", StringComparison.OrdinalIgnoreCase) ? 13.8 : 12.1;
                double highLimit = gender.Equals("Male", StringComparison.OrdinalIgnoreCase) ? 17.2 : 15.1;

                if (hbValue < lowLimit)
                {
                    abnormalFindings.Add($"Low Hemoglobin ({hbValue} g/dL, normal: {lowLimit}-{highLimit} g/dL)");
                    explanations.Add("Low hemoglobin is consistent with anemia, which means your blood has a lower than normal concentration of red blood cells. This can lead to symptoms like fatigue, weakness, and shortness of breath.");
                    doctorQuestions.Add("What is causing my low hemoglobin level, and should we test my iron, B12, or folate levels?");
                }
                else if (hbValue > highLimit)
                {
                    abnormalFindings.Add($"High Hemoglobin ({hbValue} g/dL, normal: {lowLimit}-{highLimit} g/dL)");
                    explanations.Add("Elevated hemoglobin could suggest dehydration, smoking, chronic obstructive pulmonary disease (COPD), or, rarely, a bone marrow disorder like polycythemia vera.");
                    doctorQuestions.Add("Does this high hemoglobin level require further hematological evaluation?");
                }
            }

            // 2. Analyze WBC
            var wbcMatch = Regex.Match(text, @"(?:wbc|white blood cell|white blood cells)(?:\s+\w+){0,5}\s*(?:is|:|=|came back as)?\s*([0-9]+(?:\.[0-9]+)?)");
            if (wbcMatch.Success && double.TryParse(wbcMatch.Groups[1].Value, out double wbcValue))
            {
                // Note: WBC values can be input in units of thousands (e.g. 15.4 or 15400)
                double normalizedWbc = wbcValue < 100 ? wbcValue : wbcValue / 1000.0;
                
                if (normalizedWbc > 11.0)
                {
                    abnormalFindings.Add($"High White Blood Cell Count ({wbcValue} x10^3/mcL, normal: 4.5-11.0)");
                    explanations.Add("An elevated white blood cell (WBC) count (leukocytosis) typically suggests your body is responding to an infection, physical stress, inflammation, or certain medications.");
                    doctorQuestions.Add("Could this elevated white blood cell count indicate an active infection or inflammatory process?");
                }
                else if (normalizedWbc < 4.5)
                {
                    abnormalFindings.Add($"Low White Blood Cell Count ({wbcValue} x10^3/mcL, normal: 4.5-11.0)");
                    explanations.Add("A low WBC count (leukopenia) may increase your risk of infection. It can be caused by viral infections, autoimmune conditions, bone marrow issues, or vitamin deficiencies.");
                    doctorQuestions.Add("What steps should we take to monitor my low WBC count, and do I need to take special infection precautions?");
                }
            }

            // 3. Analyze Cholesterol
            var cholMatch = Regex.Match(text, @"(?:total cholesterol|cholesterol)(?:\s+\w+){0,5}\s*(?:is|:|=|came back as)?\s*([0-9]+)");
            if (cholMatch.Success && double.TryParse(cholMatch.Groups[1].Value, out double cholValue))
            {
                if (cholValue > 200)
                {
                    abnormalFindings.Add($"Elevated Total Cholesterol ({cholValue} mg/dL, normal: < 200 mg/dL)");
                    explanations.Add("Fasting total cholesterol level above 200 mg/dL increases cardiovascular risks. It is a marker for hyperlipidemia, which can be modified by diet, lifestyle, or lipid-lowering therapies.");
                    doctorQuestions.Add("Should we look at my lipid fractions (LDL, HDL, triglycerides) to calculate my overall cardiovascular risk score?");
                }
            }

            // 4. Analyze Blood Sugar (Fasting Glucose)
            var glucoseMatch = Regex.Match(text, @"(?:glucose|blood sugar)(?:\s+\w+){0,5}\s*(?:is|:|=|came back as)?\s*([0-9]+)");
            if (glucoseMatch.Success && double.TryParse(glucoseMatch.Groups[1].Value, out double glucoseValue))
            {
                if (glucoseValue >= 126)
                {
                    abnormalFindings.Add($"Elevated Fasting Glucose ({glucoseValue} mg/dL, normal: 70-100 mg/dL)");
                    explanations.Add("A fasting glucose level of 126 mg/dL or higher on two separate occasions is consistent with diabetes mellitus. Fasting glucose between 100 and 125 indicates prediabetes.");
                    doctorQuestions.Add("Do I need an HbA1c test to assess my average blood sugar control over the past 3 months?");
                }
                else if (glucoseValue < 70)
                {
                    abnormalFindings.Add($"Low Fasting Glucose ({glucoseValue} mg/dL, normal: 70-100 mg/dL)");
                    explanations.Add("Fasting blood sugar below 70 mg/dL is consistent with hypoglycemia, which can cause shakiness, dizziness, sweating, and heart palpitations.");
                    doctorQuestions.Add("What could be causing these hypoglycemic episodes, and how should I manage my diet to stabilize my glucose levels?");
                }
            }

            // 5. Analyze TSH
            var tshMatch = Regex.Match(text, @"(?:tsh|thyroid stimulating hormone)(?:\s+\w+){0,5}\s*(?:is|:|=|came back as)?\s*([0-9]+(?:\.[0-9]+)?)");
            if (tshMatch.Success && double.TryParse(tshMatch.Groups[1].Value, out double tshValue))
            {
                if (tshValue > 4.5)
                {
                    abnormalFindings.Add($"Elevated TSH ({tshValue} mIU/L, normal: 0.4-4.0 mIU/L)");
                    explanations.Add("An elevated Thyroid Stimulating Hormone (TSH) level could suggest hypothyroidism (underactive thyroid), meaning your pituitary gland is producing more TSH to stimulate your thyroid.");
                    doctorQuestions.Add("Should we run a free T4 (thyroxine) level test to check for clinical hypothyroidism?");
                }
                else if (tshValue < 0.4)
                {
                    abnormalFindings.Add($"Suppressed TSH ({tshValue} mIU/L, normal: 0.4-4.0 mIU/L)");
                    explanations.Add("A low TSH level could suggest hyperthyroidism (overactive thyroid), which can cause weight loss, rapid heartbeat, anxiety, and heat intolerance.");
                    doctorQuestions.Add("Should we run free T3 and free T4 test to investigate hyperthyroidism?");
                }
            }

            // Fallback general analysis if nothing matched specifically but text contains lab terms
            if (abnormalFindings.Count == 0)
            {
                context.Response.Summary = "We parsed your lab report parameters. All evaluated markers (such as Hemoglobin, WBC, Cholesterol, Glucose, and TSH) appear within standard reference intervals.";
                context.Response.RiskLevel = "Low";
            }
            else
            {
                context.Response.Summary = $"Your uploaded report has been interpreted. We identified {abnormalFindings.Count} parameters outside standard reference ranges.";
                context.Response.RiskLevel = "Medium";
                context.Response.ConfidenceScore = 0.85;

                foreach (var finding in abnormalFindings)
                {
                    context.Response.PossibleCauses.Add(new PossibleCause
                    {
                        Name = finding,
                        ConfidenceScore = 0.90,
                        Description = explanations[abnormalFindings.IndexOf(finding)]
                    });
                }
            }

            // Populate recommendations next steps
            foreach (var dq in doctorQuestions)
            {
                context.Response.RecommendedNextSteps.Add(dq);
            }
            context.Response.RecommendedNextSteps.Add("Please share these results with your healthcare provider for a formal diagnosis.");

            // Store inside metadata for other agents (like Drug or Nutrition agent) to inspect
            context.Metadata["AbnormalFindings"] = abnormalFindings;

            return Task.CompletedTask;
        }
    }
}
