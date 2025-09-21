using System;
using System.Collections.Generic;

namespace ERP_AI.Desktop.Services
{
    public class HelpTopic
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        public DateTime CreatedDate { get; set; }
        public DateTime LastModified { get; set; }
        public int ViewCount { get; set; }
        public bool IsPublished { get; set; }
    }

    public class Tutorial
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<TutorialStep> Steps { get; set; } = new List<TutorialStep>();
        public TimeSpan EstimatedDuration { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public List<string> Prerequisites { get; set; } = new List<string>();
        public bool IsCompleted { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class TutorialStep
    {
        public int StepNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }

    public class HelpContent
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> RelatedTopics { get; set; } = new List<string>();
    }

    public class HelpTip
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public bool IsDismissed { get; set; }
    }

    public class VideoHelp
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public string ThumbnailUrl { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
    }

    public class GettingStartedGuide
    {
        public List<GettingStartedStep> Steps { get; set; } = new List<GettingStartedStep>();
        public int CurrentStep { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsSkipped { get; set; }
    }

    public class GettingStartedStep
    {
        public int StepNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }

    public class HelpSettings
    {
        public bool ShowTooltips { get; set; } = true;
        public bool ShowContextHelp { get; set; } = true;
        public bool EnableOfflineHelp { get; set; } = true;
        public string PreferredLanguage { get; set; } = "en";
        public bool AutoUpdateHelp { get; set; } = true;
    }
}
