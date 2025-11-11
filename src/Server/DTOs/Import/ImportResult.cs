namespace Server.DTOs.Import;

public class ImportResult
{
    public int Total { get; set; }
    public int Creados { get; set; }
    public int Actualizados { get; set; }
    public int Errores { get; set; }
    public List<string> Mensajes { get; set; } = new();
    public string? LogFilePath { get; set; }
}
