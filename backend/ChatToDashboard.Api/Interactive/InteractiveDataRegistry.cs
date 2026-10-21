namespace ChatToDashboard.Api.Interactive;

/// <summary>Looks up the right <see cref="IInteractiveDataProvider"/> by RenderKind — see
/// Controllers/InteractiveController.cs, the only caller.</summary>
public class InteractiveDataRegistry
{
    private readonly Dictionary<string, IInteractiveDataProvider> _byTemplate;

    public InteractiveDataRegistry(IEnumerable<IInteractiveDataProvider> providers)
    {
        _byTemplate = providers.ToDictionary(p => p.TemplateId, StringComparer.OrdinalIgnoreCase);
    }

    public IInteractiveDataProvider? Find(string templateId) => _byTemplate.GetValueOrDefault(templateId);
}

/// <summary>See the doc comment on <see cref="IInteractiveDataProvider"/> for why this
/// stub exists for the pre-existing network/workflow/marketplace templates — their real
/// per-project SQL mapping is future work, not part of this task's assigned scope.</summary>
public sealed class StubInteractiveDataProvider : IInteractiveDataProvider
{
    public string TemplateId { get; }
    private readonly IReadOnlyList<string> _missingSources;

    public StubInteractiveDataProvider(string templateId, IReadOnlyList<string> missingSources)
    {
        TemplateId = templateId;
        _missingSources = missingSources;
    }

    public Task<InteractiveDataResult> BuildAsync(string projectId, CancellationToken ct) =>
        Task.FromResult(InteractiveDataResult.Unlinked(_missingSources));
}
