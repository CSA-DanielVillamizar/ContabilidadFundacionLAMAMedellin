using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

namespace ContabilidadLAMAMedellin.Tests.E2E;

/// <summary>
/// Helpers de utilidad para formularios MudBlazor en tests E2E Playwright.
/// Incluye métodos para interactuar con MudTextField, MudSelect, MudDatePicker, etc.
/// </summary>
public static class MudBlazorHelpers
{
    /// <summary>
    /// Rellena un MudTextField por su Label (busca el label y luego el input asociado).
    /// </summary>
    public static async Task FillMudTextFieldAsync(IPage page, string label, string value)
    {
        // MudBlazor: el label está dentro de .mud-input-label, el input es un <input> hermano
        var input = page.Locator($"label.mud-input-label:has-text('{label}')").Locator("xpath=following-sibling::input | xpath=../input");
        await input.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await input.FillAsync(value);
    }

    /// <summary>
    /// Selecciona una opción de un MudSelect por su Label.
    /// </summary>
    public static async Task SelectMudSelectAsync(IPage page, string label, string optionText)
    {
        // Hacer clic en el MudSelect para abrir el dropdown
        var select = page.Locator($"label.mud-input-label:has-text('{label}')").Locator("xpath=following-sibling::div[contains(@class,'mud-select')] | xpath=../div[contains(@class,'mud-select')]");
        await select.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await select.ClickAsync();

        // Esperar que aparezca el popover con las opciones
        await page.WaitForSelectorAsync(".mud-popover-open .mud-list-item", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 3000 });

        // Hacer clic en la opción deseada
        await page.ClickAsync($".mud-popover-open .mud-list-item:has-text('{optionText}')");
    }

    /// <summary>
    /// Rellena un MudDatePicker por su Label.
    /// </summary>
    public static async Task FillMudDatePickerAsync(IPage page, string label, string date)
    {
        // MudDatePicker: buscar input asociado y rellenar directamente (formato yyyy-MM-dd)
        var input = page.Locator($"label.mud-input-label:has-text('{label}')").Locator("xpath=following-sibling::input | xpath=../input");
        await input.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await input.FillAsync(date);
    }

    /// <summary>
    /// Hace clic en un MudCheckBox por su Label.
    /// </summary>
    public static async Task ClickMudCheckBoxAsync(IPage page, string label)
    {
        // MudCheckBox: buscar el input[type=checkbox] dentro del span con label
        var checkbox = page.Locator($"label:has-text('{label}')").Locator("xpath=preceding-sibling::input[@type='checkbox'] | xpath=../input[@type='checkbox']");
        await checkbox.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await checkbox.ClickAsync();
    }

    /// <summary>
    /// Hace clic en un MudButton por su texto.
    /// </summary>
    public static async Task ClickMudButtonAsync(IPage page, string buttonText)
    {
        var button = page.Locator($"button.mud-button:has-text('{buttonText}')");
        await button.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await button.ClickAsync();
    }

    /// <summary>
    /// Espera a que aparezca un MudDialog con un título específico.
    /// </summary>
    public static async Task WaitForMudDialogAsync(IPage page, string? titleText = null)
    {
        var selector = titleText != null
            ? $".mud-dialog .mud-dialog-title:has-text('{titleText}')"
            : ".mud-dialog";
        await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
    }

    /// <summary>
    /// Cierra un MudDialog haciendo clic en Cancelar.
    /// </summary>
    public static async Task CloseMudDialogAsync(IPage page)
    {
        await ClickMudButtonAsync(page, "Cancelar");
    }

    /// <summary>
    /// Espera a que aparezca un MudSnackbar con un mensaje específico.
    /// </summary>
    public static async Task<bool> WaitForMudSnackbarAsync(IPage page, string? messageText = null, int timeoutMs = 5000)
    {
        try
        {
            var selector = messageText != null
                ? $".mud-snackbar:has-text('{messageText}')"
                : ".mud-snackbar";
            await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Hace clic en un botón MudBlazor cuando se habilita.
    /// Reintenta hasta que el botón esté habilitado (sin atributo disabled) o se alcance el timeout.
    /// </summary>
    public static async Task ClickMudButtonWhenEnabledAsync(IPage page, string buttonText, int timeoutMs = 5000)
    {
        var button = page.Locator($"button.mud-button:has-text('{buttonText}')").First;
        await button.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        
        var fin = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < fin)
        {
            var disabled = await button.GetAttributeAsync("disabled");
            if (disabled == null)
            {
                await button.ClickAsync();
                return;
            }
            await Task.Delay(200);
        }
        throw new TimeoutException($"El botón '{buttonText}' no se habilitó tras {timeoutMs}ms");
    }

    /// <summary>
    /// Espera a que un MudTable cargue datos (verifica que tenga filas visibles).
    /// </summary>
    public static async Task WaitForMudTableDataAsync(IPage page, string? tableSelector = null, int timeoutMs = 5000)
    {
        var selector = tableSelector != null
            ? $"{tableSelector} tbody tr"
            : ".mud-table tbody tr";
        await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
    }

    /// <summary>
    /// Hace clic en el botón "Editar" de una fila de MudTable que contiene el texto especificado.
    /// </summary>
    public static async Task ClickTableEditButtonAsync(IPage page, string rowText)
    {
        var row = page.Locator($"tr:has-text('{rowText}')");
        await row.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
        
        var editButton = row.Locator("button[title='Editar'], button:has-text('Editar'), button .mud-icon-root:has-text('edit')");
        await editButton.ClickAsync();
    }

    /// <summary>
    /// Hace clic en el botón "Eliminar" de una fila de MudTable que contiene el texto especificado.
    /// </summary>
    public static async Task ClickTableDeleteButtonAsync(IPage page, string rowText)
    {
        var row = page.Locator($"tr:has-text('{rowText}')");
        await row.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
        
        var deleteButton = row.Locator("button[title='Eliminar'], button:has-text('Eliminar'), button .mud-icon-root:has-text('delete')");
        await deleteButton.ClickAsync();
    }

    /// <summary>
    /// Busca una fila de MudTable que contenga el texto especificado y verifica que existe.
    /// </summary>
    public static async Task<bool> TableContainsRowAsync(IPage page, string rowText)
    {
        var row = page.Locator($".mud-table tbody tr:has-text('{rowText}')");
        return await row.CountAsync() > 0;
    }

    /// <summary>
    /// Rellena un MudNumericField por su Label.
    /// </summary>
    public static async Task FillMudNumericFieldAsync(IPage page, string label, string value)
    {
        // MudNumericField se comporta como MudTextField
        await FillMudTextFieldAsync(page, label, value);
    }

    /// <summary>
    /// Rellena un MudTextField multiline (textarea) por su Label.
    /// </summary>
    public static async Task FillMudTextAreaAsync(IPage page, string label, string value)
    {
        var textarea = page.Locator($"label.mud-input-label:has-text('{label}')").Locator("xpath=following-sibling::textarea | xpath=../textarea");
        await textarea.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await textarea.FillAsync(value);
    }

    /// <summary>
    /// Rellena un MudAutocomplete por su Label y selecciona la primera opción coincidente.
    /// </summary>
    public static async Task FillMudAutocompleteAsync(IPage page, string label, string searchText)
    {
        var input = page.Locator($"label.mud-input-label:has-text('{label}')").Locator("xpath=following-sibling::input | xpath=../input");
        await input.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await input.FillAsync(searchText);
        await Task.Delay(500); // Esperar a que se carguen las opciones

        // Esperar y hacer clic en la primera opción del popover
        await page.WaitForSelectorAsync(".mud-popover-open .mud-list-item", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 3000 });
        await page.ClickAsync(".mud-popover-open .mud-list-item >> nth=0");
    }

    /// <summary>
    /// Verifica si un MudTextField contiene un error de validación.
    /// </summary>
    public static async Task<bool> HasValidationErrorAsync(IPage page, string label)
    {
        var errorElement = page.Locator($"label.mud-input-label:has-text('{label}')").Locator("xpath=following-sibling::p[contains(@class,'mud-input-error')] | xpath=../p[contains(@class,'mud-input-error')]");
        var count = await errorElement.CountAsync();
        return count > 0;
    }

    /// <summary>
    /// Obtiene el texto del mensaje de error de validación de un MudTextField.
    /// </summary>
    public static async Task<string?> GetValidationErrorMessageAsync(IPage page, string label)
    {
        var errorElement = page.Locator($"label.mud-input-label:has-text('{label}')").Locator("xpath=following-sibling::p[contains(@class,'mud-input-error')] | xpath=../p[contains(@class,'mud-input-error')]");
        var count = await errorElement.CountAsync();
        return count > 0 ? await errorElement.TextContentAsync() : null;
    }

    /// <summary>
    /// Hace clic en un MudIconButton por su título o ícono.
    /// </summary>
    public static async Task ClickMudIconButtonAsync(IPage page, string titleOrIcon)
    {
        var button = page.Locator($"button.mud-icon-button[title='{titleOrIcon}'], button.mud-icon-button:has(.mud-icon-root:has-text('{titleOrIcon}'))");
        await button.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await button.ClickAsync();
    }

    /// <summary>
    /// Hace clic en un tab de MudTabs por su texto.
    /// </summary>
    public static async Task ClickMudTabAsync(IPage page, string tabText)
    {
        var tab = page.Locator($".mud-tabs .mud-tab:has-text('{tabText}')");
        await tab.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await tab.ClickAsync();
    }

    /// <summary>
    /// Espera a que un MudProgressCircular o MudProgressLinear desaparezca (indicando fin de carga).
    /// </summary>
    public static async Task WaitForLoadingToFinishAsync(IPage page, int timeoutMs = 10000)
    {
        try
        {
            await page.WaitForSelectorAsync(".mud-progress-circular, .mud-progress-linear", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden, Timeout = timeoutMs });
        }
        catch
        {
            // Si no hay indicador de carga, continuar
        }
    }

    /// <summary>
    /// Filtra una MudTable usando un MudTextField de búsqueda.
    /// </summary>
    public static async Task FilterMudTableAsync(IPage page, string searchLabel, string searchText)
    {
        await FillMudTextFieldAsync(page, searchLabel, searchText);
        await Task.Delay(500); // Esperar a que se aplique el filtro
    }

    /// <summary>
    /// Cuenta el número de filas visibles en una MudTable.
    /// </summary>
    public static async Task<int> CountMudTableRowsAsync(IPage page, string? tableSelector = null)
    {
        var selector = tableSelector != null
            ? $"{tableSelector} tbody tr"
            : ".mud-table tbody tr";
        return await page.Locator(selector).CountAsync();
    }

    /// <summary>
    /// Hace clic en el botón de paginación "Siguiente" de una MudTable.
    /// </summary>
    public static async Task ClickMudTableNextPageAsync(IPage page)
    {
        var nextButton = page.Locator(".mud-table-pagination button[aria-label='Next page']");
        await nextButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await nextButton.ClickAsync();
    }

    /// <summary>
    /// Hace clic en el botón de paginación "Anterior" de una MudTable.
    /// </summary>
    public static async Task ClickMudTablePreviousPageAsync(IPage page)
    {
        var prevButton = page.Locator(".mud-table-pagination button[aria-label='Previous page']");
        await prevButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await prevButton.ClickAsync();
    }

    /// <summary>
    /// Verifica si un MudButton está deshabilitado.
    /// </summary>
    public static async Task<bool> IsMudButtonDisabledAsync(IPage page, string buttonText)
    {
        var button = page.Locator($"button.mud-button:has-text('{buttonText}')");
        var disabled = await button.GetAttributeAsync("disabled");
        return disabled != null;
    }

    /// <summary>
    /// Espera a que un elemento MudBlazor esté visible.
    /// </summary>
    public static async Task WaitForElementAsync(IPage page, string selector, int timeoutMs = 5000)
    {
        await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
    }

    /// <summary>
    /// Hace clic en un MudChip por su texto.
    /// </summary>
    public static async Task ClickMudChipAsync(IPage page, string chipText)
    {
        var chip = page.Locator($".mud-chip:has-text('{chipText}')");
        await chip.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await chip.ClickAsync();
    }

    /// <summary>
    /// Obtiene el valor actual de un MudTextField por su label.
    /// </summary>
    public static async Task<string?> GetMudTextFieldValueAsync(IPage page, string label)
    {
        var input = page.Locator($"label.mud-input-label:has-text('{label}')").Locator("xpath=following-sibling::input | xpath=../input");
        return await input.InputValueAsync();
    }

    /// <summary>
    /// Verifica si un MudDialog está visible.
    /// </summary>
    public static async Task<bool> IsMudDialogVisibleAsync(IPage page, string? titleText = null)
    {
        var selector = titleText != null
            ? $".mud-dialog .mud-dialog-title:has-text('{titleText}')"
            : ".mud-dialog";
        return await page.IsVisibleAsync(selector);
    }

    /// <summary>
    /// Hace clic en el botón "Guardar" de un MudDialog.
    /// </summary>
    public static async Task SaveMudDialogAsync(IPage page)
    {
        await ClickMudButtonAsync(page, "Guardar");
        await Task.Delay(500); // Esperar a que se procese el guardado
    }

    /// <summary>
    /// Hace clic en el botón "Aceptar" de un MudDialog de confirmación.
    /// </summary>
    public static async Task ConfirmMudDialogAsync(IPage page)
    {
        await ClickMudButtonAsync(page, "Aceptar");
        await Task.Delay(500);
    }

    /// <summary>
    /// Espera a que un MudDialog se cierre.
    /// </summary>
    public static async Task WaitForMudDialogToCloseAsync(IPage page, int timeoutMs = 5000)
    {
        await page.WaitForSelectorAsync(".mud-dialog", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden, Timeout = timeoutMs });
    }
}
