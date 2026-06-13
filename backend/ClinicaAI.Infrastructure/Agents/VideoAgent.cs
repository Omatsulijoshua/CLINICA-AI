using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicaAI.Core.Interfaces;
using ClinicaAI.Core.Models;

namespace ClinicaAI.Infrastructure.Agents
{
    public class VideoAgent : IMedicalAgent
    {
        public string Name => "Video Agent";

        public Task ExecuteAsync(AgentExecutionContext context)
        {
            var query = context.UserInput.ToLowerInvariant();
            var videos = new List<VideoRecommendation>();

            if (query.Contains("malaria"))
            {
                videos.Add(new VideoRecommendation
                {
                    Title = "What is Malaria? Symptoms, Treatment & Prevention",
                    Channel = "Osmosis from Elsevier",
                    Duration = "7:45",
                    Link = "https://www.youtube.com/watch?v=F3aO2m00sXo",
                    ThumbnailUrl = "https://img.youtube.com/vi/F3aO2m00sXo/hqdefault.jpg"
                });
                videos.Add(new VideoRecommendation
                {
                    Title = "How Malaria Parasites Infect Humans",
                    Channel = "Nature Video",
                    Duration = "4:12",
                    Link = "https://www.youtube.com/watch?v=1v55h1M4Qk0",
                    ThumbnailUrl = "https://img.youtube.com/vi/1v55h1M4Qk0/hqdefault.jpg"
                });
            }
            else if (query.Contains("lyme") || query.Contains("tick"))
            {
                videos.Add(new VideoRecommendation
                {
                    Title = "Lyme Disease: Symptoms, Diagnosis, and Treatment",
                    Channel = "Mayo Clinic",
                    Duration = "3:22",
                    Link = "https://www.youtube.com/watch?v=yGZl51WjU3Y",
                    ThumbnailUrl = "https://img.youtube.com/vi/yGZl51WjU3Y/hqdefault.jpg"
                });
                videos.Add(new VideoRecommendation
                {
                    Title = "Lyme Disease: What You Need to Know",
                    Channel = "Nucleus Medical Media",
                    Duration = "5:18",
                    Link = "https://www.youtube.com/watch?v=5rT8vC0OqKk",
                    ThumbnailUrl = "https://img.youtube.com/vi/5rT8vC0OqKk/hqdefault.jpg"
                });
            }
            else if (query.Contains("cholesterol") || query.Contains("lipid"))
            {
                videos.Add(new VideoRecommendation
                {
                    Title = "High Cholesterol - Causes, Symptoms, and Treatment",
                    Channel = "Revising Medicine",
                    Duration = "6:30",
                    Link = "https://www.youtube.com/watch?v=83cZc6ClyE8",
                    ThumbnailUrl = "https://img.youtube.com/vi/83cZc6ClyE8/hqdefault.jpg"
                });
            }
            else if (query.Contains("hypertension") || query.Contains("blood pressure"))
            {
                videos.Add(new VideoRecommendation
                {
                    Title = "Hypertension (High Blood Pressure) - Osmosis Pathophysiology",
                    Channel = "Osmosis from Elsevier",
                    Duration = "9:15",
                    Link = "https://www.youtube.com/watch?v=3n5MpHc2q-o",
                    ThumbnailUrl = "https://img.youtube.com/vi/3n5MpHc2q-o/hqdefault.jpg"
                });
            }
            else // Default general healthcare education
            {
                videos.Add(new VideoRecommendation
                {
                    Title = "Understanding the Human Immune System",
                    Channel = "Kurzgesagt – In a Nutshell",
                    Duration = "10:25",
                    Link = "https://www.youtube.com/watch?v=zQGOcOUBi6s",
                    ThumbnailUrl = "https://img.youtube.com/vi/zQGOcOUBi6s/hqdefault.jpg"
                });
            }

            foreach (var vid in videos)
            {
                context.Response.EducationalVideos.Add(vid);
            }

            return Task.CompletedTask;
        }
    }
}
