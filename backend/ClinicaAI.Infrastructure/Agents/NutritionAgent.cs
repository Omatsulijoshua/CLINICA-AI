using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicaAI.Core.Interfaces;
using ClinicaAI.Core.Models;

namespace ClinicaAI.Infrastructure.Agents
{
    public class NutritionAgent : IMedicalAgent
    {
        public string Name => "Nutrition Agent";

        public Task ExecuteAsync(AgentExecutionContext context)
        {
            var text = context.UserInput.ToLowerInvariant();
            var nutrition = new List<string>();
            var lifestyle = new List<string>();

            // Retrieve findings/profile indicators
            var conditions = context.Metadata.GetValueOrDefault("KnownConditions", "") as string ?? "";
            conditions = conditions.ToLowerInvariant();

            // 1. Check for Hypertension (either in user query, profile or history)
            bool isHypertensive = text.Contains("hypertension") || text.Contains("high blood pressure") || conditions.Contains("hypertension");
            if (isHypertensive)
            {
                nutrition.Add("Follow the DASH (Dietary Approaches to Stop Hypertension) diet: rich in fruits, vegetables, whole grains, and low-fat dairy.");
                nutrition.Add("Restrict sodium intake to less than 2,000 mg per day (ideally 1,500 mg). Avoid processed foods, canned soups, and cured meats.");
                lifestyle.Add("Engage in at least 150 minutes of moderate-intensity aerobic exercise per week (e.g., brisk walking).");
                lifestyle.Add("Monitor your blood pressure daily at home and record readings for your doctor.");
            }

            // 2. Check for High Cholesterol / Lipid Panels
            bool isHighCholesterol = text.Contains("cholesterol") || text.Contains("lipid") || text.Contains("hyperlipidemia");
            if (isHighCholesterol)
            {
                nutrition.Add("Incorporate soluble fiber (oats, barley, beans, lentils, Brussels sprouts) which binds cholesterol in the digestive system.");
                nutrition.Add("Reduce saturated fats (fatty meats, butter, cheese) and eliminate trans fats. Replace with healthy monounsaturated fats (olive oil, avocados).");
                lifestyle.Add("Add cardioprotective exercise like swimming, cycling, or jogging 3-4 times a week.");
                lifestyle.Add("Avoid smoking, as tobacco smoke lowers HDL ('good') cholesterol and increases cardiovascular risk.");
            }

            // 3. Check for High Blood Sugar / Diabetes
            bool isDiabetic = text.Contains("diabetes") || text.Contains("glucose") || text.Contains("sugar") || conditions.Contains("diabetes");
            if (isDiabetic)
            {
                nutrition.Add("Focus on low-glycemic index (GI) foods (e.g., steel-cut oats, non-starchy vegetables, legumes) to prevent rapid glucose spikes.");
                nutrition.Add("Practice carbohydrate counting and distribute carb intake evenly across balanced meals with fiber and protein.");
                lifestyle.Add("Perform regular post-meal walks (10-15 minutes) to improve insulin sensitivity and reduce postprandial blood sugar spikes.");
            }

            // 4. Check for General Infection / Fever
            bool hasFeverOrCold = text.Contains("fever") || text.Contains("cough") || text.Contains("cold") || text.Contains("malaria");
            if (hasFeverOrCold)
            {
                nutrition.Add("Ensure high oral hydration: drink water, herbal teas, or oral rehydration solutions to replace lost fluids from sweating.");
                nutrition.Add("Consume nutrient-dense, easily digestible foods such as broths, soups, bananas, and cooked grains.");
                lifestyle.Add("Prioritize absolute physical rest. Avoid strenuous workouts, as the body requires significant energy to battle infection.");
                lifestyle.Add("Keep the room at a comfortable temperature and use a cool compress to help manage mild fever discomfort.");
            }

            // Default general tips if no specific condition triggered
            if (nutrition.Count == 0)
            {
                nutrition.Add("Maintain a balanced diet consisting of whole foods, colorful vegetables, lean proteins, and fiber-rich complex carbohydrates.");
                nutrition.Add("Limit added sugars, highly processed snacks, and sweetened beverages.");
                lifestyle.Add("Aim for 7-8 hours of quality sleep per night to support immune function and cognitive health.");
                lifestyle.Add("Incorporate daily stress-reduction practices like mindfulness, deep breathing, or light stretching.");
            }

            // Populate recommendations
            foreach (var nut in nutrition)
            {
                context.Response.NutritionSuggestions.Add(nut);
            }
            foreach (var life in lifestyle)
            {
                context.Response.LifestyleRecommendations.Add(life);
            }

            return Task.CompletedTask;
        }
    }
}
