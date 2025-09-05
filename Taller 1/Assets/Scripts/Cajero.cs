using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.IO;
using cliente;
using UnityEngine.UI;

public class Cajero : MonoBehaviour
{
    [Header("UI Cajeros")]
    public TMP_Text cajero1;
    public TMP_Text cajero2;
    public TMP_Text cajero3;
    public TMP_Text cajero4;

    [Header("UI Clientes")]
    public TMP_Text clientes;

    [Header("UI Clientes - Área de Imágenes")]
    public Transform clientesImageContainer;
    public TMP_Text clientesCountText;

    [Header("Imágenes de Clientes")]
    public Texture2D[] clienteImages;

    private Queue<Cliente> colaClientes = new Queue<Cliente>();
    private Queue<RawImage> colaImagenes = new Queue<RawImage>();
    private List<Cliente> listaClientes; 
    private List<string>[] nombresAtendidos = new List<string>[4];
    private int clienteIndex = 0; 
    private int contadorClientes = 1;

    private bool[] ocupado = new bool[4];
    private int[] atendidos = new int[4];
    private float[] tiempoTotal = new float[4];

    private bool enMarcha = false;
    private int consignaciones = 0;
    private int retiros = 0;

    void Start()
    {
        CargarClientesDesdeJSON(); 
        ActualizarUI();
        for (int i = 0; i < nombresAtendidos.Length; i++)
            nombresAtendidos[i] = new List<string>();
        
    }


    public void Iniciar()
    {
        StopAllCoroutines();
        enMarcha = true;
        
        StartCoroutine(GenerarClientesDesdeArchivo()); 
        StartCoroutine(EnviarClientes());
    }

    public void Detener()
    {
        enMarcha = false;
        StopAllCoroutines();

        Debug.Log("La simulación se detuvo correctamente.");

    }

    public void GenerarReporte()
    {
        Reporte reporte = new Reporte();
        // Clientes en cola
        reporte.clientesEnCola = colaClientes.Count;

        // Total consignaciones
        reporte.totalConsignaciones = consignaciones;

        // Retiros
        reporte.totalRetiros = retiros;

        // informacion por cajero
        reporte.cajeros = new List<Reporte.CajeroInfo>();
        for (int i = 0; i < 4; i++)
        {
            Reporte.CajeroInfo info = new Reporte.CajeroInfo();
            info.id = i + 1;
            info.atendidos = atendidos[i];      // Atendidos
            info.tiempoTotal = tiempoTotal[i];  // TiempoTotal
            info.nombresAtendidos = nombresAtendidos[i];
            reporte.cajeros.Add(info);

        }

        // Convertir a JSON y guardar archivo
        string json = JsonUtility.ToJson(reporte, true);
        string path = Path.Combine(Application.streamingAssetsPath, "reporte.json");
        File.WriteAllText(path, json);

        Debug.Log("Reporte generado en StreamingAssets: " + path);

    }



    private void CargarClientesDesdeJSON()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Clientes.json");
        Debug.Log("Buscando archivo en: " + path);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("El archivo Clientes.json está vacío");
                return;
            }

            ClientesData data = JsonUtility.FromJson<ClientesData>(json);
            if (data == null || data.clientes == null)
            {
                Debug.LogError("El JSON no coincide con la estructura de ClientesData");
                return;
            }

            listaClientes = new List<Cliente>();
            foreach (var c in data.clientes)
            {
                Cliente nuevo = new Cliente(c.nombre, c.correo, c.direccion, c.tramite, 0f);
                listaClientes.Add(nuevo);
            }

            Debug.Log("Clientes cargados: " + listaClientes.Count);
        }
        else
        {
            Debug.LogError("No se encontró Clientes.json en StreamingAssets");
        }
    }

    private IEnumerator GenerarClientesDesdeArchivo()
    {
        Debug.Log("Coroutine GenerarClientesDesdeArchivo iniciada");

        Debug.Log("Coroutine GenerarClientesDesdeArchivo iniciada");

    while (enMarcha)
    {
        // Generar entre 1 y 3 clientes cada segundo
        int cantidad = Random.Range(1, 4);

        for (int i = 0; i < cantidad; i++)
        {
            // Selecciona un cliente aleatorio de la lista original
            Cliente baseCliente = listaClientes[Random.Range(0, listaClientes.Count)];

            // Crear una copia del cliente con nuevo ID y tiempo aleatorio
            Cliente nuevo = new Cliente(
                baseCliente.nombre,
                baseCliente.correo,
                baseCliente.direccion,
                baseCliente.tramite,
                Random.Range(2f, 5f) // tiempo de atención aleatorio
            );

            // Asignar un ID secuencial único
            contadorClientes++;
            nuevo.idCliente = "C" + contadorClientes.ToString("00");

            // Encolar
            colaClientes.Enqueue(nuevo);

            //Encolar Img
            RawImage imagen = CrearImagenCliente(nuevo);
            colaImagenes.Enqueue(imagen);


            }

        ActualizarUI();
        yield return new WaitForSeconds(1f);
    }

        
    }
    private RawImage CrearImagenCliente(Cliente cliente)
    {
        if (clientesImageContainer == null) return null;

        GameObject imageObj = new GameObject("ClienteImage_" + cliente.idCliente);
        imageObj.transform.SetParent(clientesImageContainer);

        RawImage rawImage = imageObj.AddComponent<RawImage>();

        if (clienteImages != null && clienteImages.Length > 0)
        {
            rawImage.texture = clienteImages[Random.Range(0, clienteImages.Length)];
        }

        LayoutElement layoutElement = imageObj.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = 60;  // Ancho deseado
        layoutElement.preferredHeight = 60; // Alto deseado

        return rawImage; //Retorna la imagen
    }

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

                    // Remover la imagen correspondiente
                    if (colaImagenes.Count > 0)
                    {
                        RawImage imagenARemover = colaImagenes.Dequeue();
                        if (imagenARemover != null)
                        {
                            Destroy(imagenARemover.gameObject);
                        }
                    }

                    StartCoroutine(AtenderCliente(indice, cliente));
                }
                else
                {
                    yield return new WaitForSeconds(1.0f);
                    continue;
                }

                ActualizarUI();
            }

            yield return new WaitForSeconds(1.0f);
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
        nombresAtendidos[cajero].Add(cliente.nombre);

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

        ActualizarEstado(0, cajero1);
        ActualizarEstado(1, cajero2);
        ActualizarEstado(2, cajero3);
        ActualizarEstado(3, cajero4);

        if (clientesCountText != null)
        {
            clientesCountText.text = $"Clientes en cola: {colaImagenes.Count}";
        }

        if (clientes != null)
        {
            string lista = "Clientes en cola:\n";
            foreach (var c in colaClientes)
            {
                lista += $"{c.idCliente} - {c.nombre} ({c.tramite})\n";
            }
            clientes.text = lista;
        }
    }
     private void ActualizarEstado(int index, TMP_Text tmp)
    {
        if (tmp == null) return;

        if (ocupado[index])
        {
            tmp.text = $"Cajero {index + 1}\nOcupado";
            tmp.color = Color.red;   // 🔴 rojo si ocupado
        }
        else
        {
            tmp.text = $"Cajero {index + 1}\nLibre";
            tmp.color = Color.green; // 🟢 verde si libre
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
            public List<string> nombresAtendidos;
        }
    }

[System.Serializable]
public class ClienteData
{
    public string nombre;
    public string correo;
    public string direccion;
    public string tramite;
}

[System.Serializable]
public class ClientesData
{
    public List<ClienteData> clientes;
}

