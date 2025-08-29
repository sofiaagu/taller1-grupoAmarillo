using UnityEngine;

public class Persona : MonoBehaviour
{
    public string nombre { get; set; }
    public string correo { get; set; }
    public string direccion { get; set; }

    public Persona(string nombre, string correo, string direccion)
    {
        this.nombre = nombre;
        this.correo = correo;
        this.direccion = direccion;
    }
    
}