using UnityEngine;

public class ClienteNuevo : Persona
{
    public string IdCliente { get; set; }
    public string Tramite { get; set; } 
    public float TiempoAtencion { get; set; }

    private static int contadorID = 0;

    public ClienteNuevo(string nombre, string correo, string direccion, string tramite, float tiempoAtencion)
        : base(nombre, correo, direccion)
    {
        contadorID++;
        this.IdCliente = $"C{contadorID}";
        this.Tramite = tramite;
        this.TiempoAtencion = tiempoAtencion;
    }

    public override string ToString()
    {
        return $"[{IdCliente}] {nombre} - {Tramite} (Tiempo: {TiempoAtencion}s)";
    }

    public int GenerarTiempoAtencion;
}