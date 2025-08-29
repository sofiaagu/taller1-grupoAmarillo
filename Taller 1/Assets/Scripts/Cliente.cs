using UnityEngine;

public class Cliente : Persona
{
    public string idCliente { get; set; }
    public string tramite { get; set; } 
    public float tiempoAtencion { get; set; }

    private static int contadorID = 0;

    public Cliente(string nombre, string correo, string direccion, string tramite, float tiempoAtencion)
        : base(nombre, correo, direccion)
    {
        contadorID++;
        this.idCliente = "C" + contadorID; 
        this.tramite = tramite;
        this.tiempoAtencion = tiempoAtencion;
    }

    public override string ToString()
    {
        return $"[{idCliente}] {nombre} - {tramite} (Tiempo: {tiempoAtencion}s)";
    }
}