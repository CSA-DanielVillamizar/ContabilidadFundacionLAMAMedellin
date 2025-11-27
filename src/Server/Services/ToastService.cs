using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Server.Components.UI;

public class ToastService
{
    public List<UIToast.ToastModel> Toasts { get; } = new();

    public void Show(string message, string variant = "info", int? durationMs = null)
    {
        var toast = new UIToast.ToastModel { Id = Guid.NewGuid(), Message = message, Variant = variant };
        Toasts.Add(toast);
        NotifyStateChangedAsync();

        // Programar cierre automático para mejorar UX
        var ms = durationMs ?? GetDefaultDuration(variant);
        _ = AutoDismissAsync(toast.Id, ms);
    }

    public void Dismiss(Guid id)
    {
        var t = Toasts.FirstOrDefault(x => x.Id == id);
        if (t != null) Toasts.Remove(t);
        NotifyStateChangedAsync();
    }

    private void NotifyStateChangedAsync()
    {
        if (StateHasChanged != null)
        {
            // Ejecutar el evento de forma asíncrona para evitar problemas de Dispatcher
            Task.Run(() =>
            {
                try { StateHasChanged.Invoke(); } catch { }
            });
        }
    }

    private async Task AutoDismissAsync(Guid id, int ms)
    {
        try
        {
            await Task.Delay(ms);
            // Si aún existe, descartarlo
            var exists = Toasts.Any(t => t.Id == id);
            if (exists) Dismiss(id);
        }
        catch { }
    }

    private static int GetDefaultDuration(string variant) => variant switch
    {
        "danger" => 6000,
        "warning" => 5000,
        _ => 3000
    };

    public event Action? StateHasChanged;
}
