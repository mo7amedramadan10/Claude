namespace ChatToDashboard.Api.Freeform;

/// <summary>
/// الترجمة الإنجليزية لنماذج تصميم حر الثابتة (الاسم والوصف وعناوين الأقسام بترتيبها) — تُعرض في معاينة المعرض عند اختيار
/// «English» وتُبنى عليها الصفحة التوضيحية الإنجليزية. المفاتيح هي مفاتيح الكتالوج نفسها؛ نموذج بلا ترجمة يُعرض بالعربية.
/// </summary>
public static class FreeformTemplateI18n
{
    public sealed record En(string Name, string Description, string[] Sections);

    public static readonly IReadOnlyDictionary<string, En> English = new Dictionary<string, En>
    {
        ["doc-memo"] = new("Internal Memo", "A formal memo with subject, background, recommendation and required action", new[] { "Subject and Header", "Background", "Analysis and Options", "Recommendation", "Required Action and Approval" }),
        ["doc-report"] = new("Periodic Performance Report", "Executive summary, KPIs from your data, analysis and recommendations", new[] { "Executive Summary", "Key Indicators", "Period Trend", "Details Table", "Recommendations and Next Steps" }),
        ["doc-minutes"] = new("Meeting Minutes", "Attendance, agenda, decisions and editable follow-up items", new[] { "Meeting Details and Attendance", "Agenda", "Discussion and Decisions", "Follow-up Items (editable)" }),
        ["doc-proposal"] = new("Proposal and Quotation", "Overview, scope, timeline, cost and terms", new[] { "Cover and Overview", "Scope of Work", "Timeline", "Cost", "Terms and Approval" }),
        ["doc-policy"] = new("Policy or Procedure", "Purpose, scope, definitions, steps and responsibilities", new[] { "Purpose and Scope", "Definitions", "Step-by-step Procedure", "Responsibilities", "Version History" }),
        ["doc-onepager"] = new("One-page Summary", "The key numbers and points on one printable page", new[] { "Title and Context", "Headline Numbers", "Current Status and Points", "Conclusion" }),
        ["sl-launch"] = new("Product Launch", "Problem, solution, market, plan and team", new[] { "Cover", "The Problem", "The Solution", "How It Works", "Market and Opportunity", "Launch Plan", "Team", "Next Step" }),
        ["sl-qbr"] = new("Quarterly Business Review", "Quarter results from your data, lessons and next quarter plan", new[] { "Cover and Agenda", "Key Results", "KPI Trend", "What Worked and What Didn't", "Risks", "Next Quarter Plan" }),
        ["sl-kickoff"] = new("Project Kickoff", "Objectives, scope, team, timeline and risks", new[] { "Cover", "Objectives", "Scope", "Team and Roles", "Timeline", "Risks", "Communication and Governance" }),
        ["sl-company"] = new("Company Profile", "Who we are, our services, our clients and why us", new[] { "Cover", "Who We Are", "Our Services", "Key Numbers", "Our Clients", "Why Us", "Contact" }),
        ["sl-training"] = new("Training Material", "Learning objectives, modular content, exercise and summary", new[] { "Cover and Objectives", "Module One", "Module Two", "Module Three", "Interactive Exercise", "Summary" }),
        ["sl-status"] = new("Project Status Report", "Overall status, progress, risks and next steps", new[] { "Overall Status", "Progress vs Plan", "Period Achievements", "Risks and Issues", "Next Steps" }),
        ["ds-landing"] = new("Landing Page", "Hero, features, testimonials, FAQ and contact form", new[] { "Hero", "Features", "How It Works", "Testimonials", "FAQ", "Contact Form" }),
        ["ds-form"] = new("Request Screen Prototype", "Interactive multi-step form with validation and review before submit", new[] { "Header and Steps", "Request Data", "Details and Attachments", "Review and Submit" }),
        ["ds-profile"] = new("Project Profile Page", "Objectives, team, timeline and editable tasks", new[] { "Overview", "Objectives", "Team", "Timeline", "Tasks (editable)" }),
        ["ds-timeline"] = new("Interactive Timeline", "Milestones with details that appear on click", new[] { "Title and Filter", "Milestones", "Milestone Details" }),
        ["ds-faq"] = new("Guide or FAQ", "Instant search, collapsible sections and a table of contents", new[] { "Search and Contents", "Sections and Questions", "Contact for Help" }),
        ["ds-catalog"] = new("Card Catalog", "A card grid with filtering, sorting and details", new[] { "Filter and Sort Bar", "Card Grid", "Details Dialog" }),
        ["db-sales"] = new("Sales Tracking", "Revenue, deals, monthly trend and distribution", new[] { "KPIs", "Monthly Trend", "Distribution", "Details Table" }),
        ["db-projects"] = new("Project Tracking", "Status, progress, risks and deadlines", new[] { "KPIs", "Distribution by Status", "Progress per Project", "Upcoming Deadlines" }),
        ["db-finance"] = new("Finance Dashboard", "Revenue, expenses, margin and cash flow", new[] { "KPIs", "Revenue vs Expenses", "Expenses by Item", "Monthly Table" }),
        ["db-hr"] = new("Human Resources", "Headcount, hiring, turnover and attendance", new[] { "KPIs", "Distribution by Department", "Composition", "Vacancies and Hiring" }),
        ["db-ops"] = new("Operations and Service", "Tickets, response time and SLAs", new[] { "KPIs", "Daily Trend", "Distribution by Category", "Critical Tickets" }),
        ["db-exec"] = new("Executive Snapshot", "The top 6 KPIs with trend on one screen", new[] { "The Six KPIs", "Main KPI Trend" }),
        ["mo-journey"] = new("Journey or Step-by-step Process", "A scene that advances step by step with an explanation per stage", new[] { "Scene", "Steps", "Controls" }),
        ["mo-explainer"] = new("Idea or Product Explainer", "Problem → solution → benefit in a short animated scene", new[] { "The Problem", "The Solution", "The Benefit", "Controls" }),
        ["mo-stats"] = new("Animated Numbers", "Counters that count up and charts drawn from your data", new[] { "Counters", "Animated Chart", "Controls" }),
        ["mo-timeline"] = new("Animated Timeline", "Milestones appearing in sequence along a timeline", new[] { "Timeline", "Milestones", "Controls" }),
        ["mo-intro"] = new("Logo Intro", "Logo, name and tagline appearing in a short animation", new[] { "Scene" }),
        ["sl-corporate-blue"] = new("Corporate Blue — Ready Deck", "Five ready slides in a corporate blue style (takeaways, roadmap, operating model, growth drivers, performance chart) that open instantly and can be edited inline or by chat", new[] { "Key Takeaways", "Strategic Roadmap", "Operating Model", "Business Growth Drivers", "Performance Overview (bar chart)" }),
    };

    /// <summary>الاسم الإنجليزي إن وُجد.</summary>
    public static string? NameEn(string key) => English.TryGetValue(key, out var e) ? e.Name : null;
    public static string? DescriptionEn(string key) => English.TryGetValue(key, out var e) ? e.Description : null;
    /// <summary>عنوان القسم الإنجليزي بترتيبه إن وُجد (null حين لا ترجمة أو اختلف عدد الأقسام).</summary>
    public static string? SectionEn(string key, int index) => English.TryGetValue(key, out var e) && index < e.Sections.Length ? e.Sections[index] : null;
}
