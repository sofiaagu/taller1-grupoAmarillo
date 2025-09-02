using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.IO;


public class Cajero : MonoBehaviour
{
    [Header("UI Cajeros")]
    public TMP_Text cajero1;
    public TMP_Text cajero2;
    public TMP_Text cajero3;
    public TMP_Text cajero4;

    [Header("UI Clientes")]
    public TMP_Text clientes;

    private Queue<Cliente> colaClientes = new Queue<Cliente>();
    private List<Cliente> listaClientes; // Lista de clientes cargados desde JSON
    private int clienteIndex = 0; // Para ir trayendo clientes del JSON

    private bool[] ocupado = new bool[4];
    private int[] atendidos = new int[4];
    private float[] tiempoTotal = new float[4];

    private bool enMarcha = false;
    private int consignaciones = 0; 
    private int retiros = 0; 
    //private int idCounter = 1;

    void Start()
    {
        CargarClientesDesdeJSON(); // agregue Aleja
        ActualizarUI();
    }

    
    public void Iniciar()
    {
        StopAllCoroutines();
        enMarcha = true;

        Debug.Log("Simulación iniciada");

        // StartCoroutine(GenerarClientes());
        StartCoroutine(GenerarClientesDesdeArchivo()); // Cambie Aleja
        StartCoroutine(EnviarClientes());
    }

    public void Detener()
    {
        enMarcha = false;
        StopAllCoroutines();
        Debug.Log("Simulación detenida");
    }


    //public void GenerarReporte()
    //{
    //    Reporte reporte = new Reporte();
    //    reporte.clientesEnCola = colaClientes.Count;
    //    reporte.cajeros = new List<Reporte.CajeroInfo>();
    //    reporte.totalConsignaciones = consignaciones;
    //    reporte.totalRetiros = retiros;

    //    for (int i = 0; i < 4; i++)
    //    {
    //        Reporte.CajeroInfo info = new Reporte.CajeroInfo();
    //        info.id = i + 1;
    //        info.atendidos = atendidos[i];
    //        info.tiempoTotal = tiempoTotal[i];
    //        reporte.cajeros.Add(info);
    //    }

    //    string json = JsonUtility.ToJson(reporte, true);
    //    File.WriteAllText(Application.dataPath + "/reporte.json", json);

    //    Debug.Log("Reporte generado en: " + Application.dataPath + "/reporte.json");
    //}

    //private IEnumerator GenerarClientes()
    //{
    //    Debug.Log("Coroutine GenerarClientes iniciada");

    //    while (enMarcha)
    //    {
    //        int cantidad = Random.Range(1, 4); // entre 1 y 3 clientes
    //        Debug.Log("Generando " + cantidad + " clientes");

    //        for (int i = 0; i < cantidad; i++)
    //        {
    //            string nombre = "Cliente " + idCounter;
    //            string correo = "cliente" + idCounter + "@mail.com";
    //            string direccion = "Calle " + Random.Range(1, 50);
    //            string tramite = Random.value > 0.5f ? "Consignar" : "Retirar";
    //            float tiempoAtencion = Random.Range(2f, 5f);

    //            Cliente nuevo = new Cliente(nombre, correo, direccion, tramite, tiempoAtencion);
    //            nuevo.idCliente = "C" + idCounter;

    //            colaClientes.Enqueue(nuevo);
    //            idCounter++;
    //        }

    //        ActualizarUI();
    //        yield return new WaitForSeconds(1f);
    //    }
    //}
 

    private void CargarClientesDesdeJSON()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Clientes.json");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            ClientesData data = JsonUtility.FromJson<ClientesData>(json);
            listaClientes = new List<Cliente>(data.clientes);
            Debug.Log("Clientes cargados: " + listaClientes.Count);
        }
        else
        {
            Debug.LogError("No se encontró el archivo de clientes en: " + path);
        }
    } // agregue Aleja

    private IEnumerator GenerarClientesDesdeArchivo()
    {
        Debug.Log("Coroutine GenerarClientesDesdeArchivo iniciada");

        while (enMarcha && clienteIndex < listaClientes.Count)
        {
            // Agregar un cliente en orden desde la lista
            Cliente nuevo = listaClientes[clienteIndex];
            nuevo.tiempoAtencion = Random.Range(2f, 5f); // asignamos tiempo aleatorio
            colaClientes.Enqueue(nuevo);
            clienteIndex++;

            ActualizarUI();
            yield return new WaitForSeconds(1f);
        }
    } // agregue Aleja

    private IEnumerator EnviarClientes()
    {
        Debug.Log("Coroutine EnviarClientes iniciada");

        while (enMarcha)
        {
            if (colaClientes.Count > 0)
            {
                Cliente cliente = colaClientes.Peek();
                int indice = BuscarCajeroDisponible();

                if (indice != -1)
                {
                    colaClientes.Dequeue();
                    StartCoroutine(AtenderCliente(indice, cliente));
                }
                else
                {
                    yield return new WaitForSeconds(0.5f);
                    continue;
                }

                ActualizarUI();
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator AtenderCliente(int cajero, Cliente cliente)
    {
        ocupado[cajero] = true;
        ActualizarUI();

        float tiempoAtencion = cliente.tiempoAtencion;
        Debug.Log($"Cajero {cajero + 1} atiende a {cliente.nombre} en {tiempoAtencion:F1}s ({cliente.tramite})");

        yield return new WaitForSeconds(tiempoAtencion);

        atendidos[cajero]++;
        tiempoTotal[cajero] += tiempoAtencion;

        // Contar trámites SOLO cuando fueron atendidos
        if (cliente.tramite == "Consignar")
            consignaciones++;
        else if (cliente.tramite == "Retirar")
            retiros++;

        ocupado[cajero] = false;
        ActualizarUI();
    }
   
    private int BuscarCajeroDisponible()
    {
        for (int i = 0; i < 4; i++)
        {
            if (!ocupado[i]) return i;
        }
        return -1;
    }

    private void ActualizarUI()
    {
        if (cajero1 != null) cajero1.text = "Cajero 1\n" + (ocupado[0] ? "Ocupado" : "Libre");
        if (cajero2 != null) cajero2.text = "Cajero 2\n" + (ocupado[1] ? "Ocupado" : "Libre");
        if (cajero3 != null) cajero3.text = "Cajero 3\n" + (ocupado[2] ? "Ocupado" : "Libre");
        if (cajero4 != null) cajero4.text = "Cajero 4\n" + (ocupado[3] ? "Ocupado" : "Libre");

        //if (clientes != null) clientes.text = "Clientes en cola: " + colaClientes.Count;

        if (clientes != null)// agregue Aleja
        {
            string lista = "Clientes en cola:\n";
            foreach (var c in colaClientes)
            {
                lista += $"{c.idCliente} - {c.nombre} ({c.tramite})\n";
            }
            clientes.text = lista;
        }
    }
}

[System.Serializable]
public class Reporte
{
    public int clientesEnCola;
    public int totalConsignaciones;
    public int totalRetiros;
    public List<CajeroInfo> cajeros;

    [System.Serializable]
    public class CajeroInfo
    {
        public int id;
        public int atendidos;
        public float tiempoTotal;
    }
}

// agregue Aleja
[System.Serializable]
public class Cliente
{
    public string idCliente;
    public string nombre;
    public string correo;
    public string direccion;
    public string tramite;
    [System.NonSerialized] public float tiempoAtencion; // agregado dinámicamente
}

[System.Serializable]
public class ClientesData
{
    public List<Cliente> clientes;
}


// using System.Collections;
// using System.Collections.Generic;
// using TMPro;
// using UnityEditor.PackageManager;
// using UnityEngine;

// public class Cajero : MonoBehaviour
// {
//     [SerializeField] private TMP_Text txtEstado; // Muestra "Ocupado" o "Disponible"

//     private Queue<Cliente> colaClientes = new Queue<Cliente>();
//     private bool ocupado = false;

//     public int NroclientesAtendidos = 0;
//     public float tiempoDeAtencionTotal = 0;
//     public bool ocupado= false; 

//     //Agregar un cliente a la cola
//     public void EncolarCliente(Cliente cliente)
//     {
//         colaClientes.Enqueue(cliente);
//         if (!ocupado)
//             StartCoroutine(AtenderClientes());
//     }

//     private IEnumerator AtenderClientes()
//     {
//         while (colaClientes.Count > 0)
//         {
//             ocupado = true;
//             ActualizarEstadoUI();

//             Cliente clienteActual = colaClientes.Dequeue();
//             float tiempoAtencion = Random.Range(2f, 5f); // tiempo aleatorio de atenci�n
//             Debug.Log($"Atendiendo a {clienteActual.Nombre} durante {tiempoAtencion:F1} segundos...");

//             // Espera simulando atenci�n
//             yield return new WaitForSeconds(tiempoAtencion);

//             tiempoAtencionTotal += tiempoAtencion;
//             NroclientesAtendidos++;
//             Debug.Log("Cliente atendido: {clienteActual.Nombre}. Total atendidos: {clientesAtendidos}");

//             ocupado = false;
//             ActualizarEstadoUI();
//         }
//     }
//     private void ActualizarEstadoUI()
//     {
//         if (txtEstado != null)
//         {
//             txtEstado.text = ocupado ? "Ocupado" : "Disponible";
//         }
//     }

//     public string GetEstadisticas()
//     {
//         return $"Clientes atendidos: {clientesAtendidos}, Tiempo total: {tiempoAtencionTotal:F1}s";
//     }
// }


