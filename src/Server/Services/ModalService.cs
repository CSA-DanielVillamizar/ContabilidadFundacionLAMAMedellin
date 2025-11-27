using Microsoft.AspNetCore.Components;
public class ModalService
{
    public bool IsOpen { get; private set; }
    public RenderFragment? Content { get; private set; }
    public RenderFragment? Header { get; private set; }
    public RenderFragment? Footer { get; private set; }
    public string? Class { get; private set; }
    public void Show(RenderFragment content, RenderFragment? header = null, RenderFragment? footer = null, string? @class = null)
    {
        Content = content;
        Header = header;
        Footer = footer;
        Class = @class;
        IsOpen = true;
        StateHasChanged?.Invoke();
    }
    public void Close()
    {
        IsOpen = false;
        StateHasChanged?.Invoke();
    }
    public event Action? StateHasChanged;
}
