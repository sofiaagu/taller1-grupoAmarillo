using UnityEngine;
using System.Collections.Generic;
using TMPro;


public class Control : MonoBehaviour
{
    public TMP_Text cajero1; 
    public TMP_Text Cajero2;  
    public TMP_Text Cajero3;  
    public TMP_Text Cajero4;   

    public TMP_Text clientes;   

    Queue<Cliente> colaClientes = new Queue<Cliente>();
    bool enMarcha = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ActualizarCola();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

     public void Iniciar()
    {
        enMarcha = true;
    }

    public void Detener()
    {
        enMarcha = false;
    }

    public void GenerarReporte()
    {
       

    }

    void ActualizarCola()
{
    clientes.text = "Cola: ";
    foreach (Cliente c in colaClientes)
    {
        clientes.text += "\n" + c.ToString();
    }
}
}