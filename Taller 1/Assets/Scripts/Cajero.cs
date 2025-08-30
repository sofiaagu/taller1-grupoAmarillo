using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;

public class Cajero : MonoBehaviour
{
    [SerializeField] private TMP_Text txtEstado; // Muestra "Ocupado" o "Disponible"

    private Queue<Cliente> colaClientes = new Queue<Cliente>();
    private bool ocupado = false;

    public int NroclientesAtendidos = 0;
    public float TiempoDeAtencion = 0;
    public bool ocupado= false; 

    //Agregar un cliente a la cola
    public void EncolarCliente(Cliente cliente)
    {
        colaClientes.Enqueue(cliente);
        if (!ocupado)
            StartCoroutine(AtenderClientes());
    }

    private IEnumerator AtenderClientes()
    {
        while (colaClientes.Count > 0)
        {
            ocupado = true;
            ActualizarEstadoUI();

            Cliente clienteActual = colaClientes.Dequeue();
            float tiempoAtencion = Random.Range(2f, 5f); // tiempo aleatorio de atención
            Debug.Log($"Atendiendo a {clienteActual.Nombre} durante {tiempoAtencion:F1} segundos...");

            // Espera simulando atención
            yield return new WaitForSeconds(tiempoAtencion);

            tiempoAtencionTotal += tiempoAtencion;
            clientesAtendidos++;
            Debug.Log("Cliente atendido: {clienteActual.Nombre}. Total atendidos: {clientesAtendidos}");

            ocupado = false;
            ActualizarEstadoUI();
        }
    }


}
