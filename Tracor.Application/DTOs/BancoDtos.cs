namespace Tradecorp.Application.DTOs;

/// <summary>
/// DTO para crear un banco
/// </summary>
public class CreateBancoRequest
{
    /// <summary>
    /// Nombre del banco
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
}

/// <summary>
/// DTO de respuesta para un banco
/// </summary>
public class BancoResponse
{
    /// <summary>
    /// ID del banco
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre del banco
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
}
