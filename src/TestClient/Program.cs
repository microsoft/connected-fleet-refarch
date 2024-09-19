
using MQTTnet.Client;
using MQTTnet;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


class Program
{
    // <summary>
    // This program code simulates 5 vehicles sending data using the MQTT protocol to Azure Event Grid.
    // It creates 5 MQTT clients, each with a unique client ID and client authentication name and launches them in parallel.
    // The certificates are loaded from the local file system (created by the setup scripts in the cert-gen folder).
    // The sample data is read from the SamplePayloads folder.
    // To use this program, set the gw_url environment variable to the hostname of the Azure Event Grid gateway.
    // </summary>    
    static async Task Main(string[] args)
    {
        string hostname = Environment.GetEnvironmentVariable("gw_url") ?? throw new ArgumentNullException("gw_url", "Environment variable 'gw_url' is not set.");

        // Read the x509 certificate path from the environment variable CERT_PATH and set to a default if null
        string x509pem = Environment.GetEnvironmentVariable("CERTS_PATH") == null ? @"../../infra/deployment/TelemetryPlatform/cert-gen/certs/" : Environment.GetEnvironmentVariable("CERTS_PATH");
        string x509key = Environment.GetEnvironmentVariable("CERTS_PATH") == null ? @"../../infra/deployment/TelemetryPlatform/cert-gen/certs/" : Environment.GetEnvironmentVariable("CERTS_PATH");         
        string[] deviceNames = { "device01", "device02", "device03", "device04", "device05"}; // Add more device names as needed

        List<Task> clientTasks = new List<Task>();

        foreach (string deviceName in deviceNames)
        {
            clientTasks.Add(CreateAndRunClientAsync(deviceName, hostname, x509pem, x509key));
        }

        await Task.WhenAll(clientTasks);
    }

    static async Task CreateAndRunClientAsync(string deviceName, string hostname, string x509_pem, string x509_key)
    {
        string pemFilePath = Path.Combine(x509_pem, $"{deviceName}.cert.pem");
        string keyFilePath = Path.Combine(x509_key, $"{deviceName}.key.pem");

        var certificate = new X509Certificate2(X509Certificate2.CreateFromPemFile(pemFilePath, keyFilePath).Export(X509ContentType.Pkcs12));

        var mqttClient = new MqttFactory().CreateMqttClient();

        var connAck = await mqttClient.ConnectAsync(new MqttClientOptionsBuilder()
            .WithTcpServer(hostname, 8883)
            .WithClientId($"{deviceName}-client") // Use a unique client ID for each device
            .WithCredentials($"{deviceName}.mqtt.contoso.com", "")  // use client authentication name in the username
            .WithTlsOptions(new MqttClientTlsOptionsBuilder()
                .WithClientCertificates(new X509Certificate2Collection(certificate))
                .Build())
            .Build());
            
        Console.WriteLine($"Device '{deviceName}': Client Connected: {mqttClient.IsConnected} with CONNACK: {connAck.ResultCode}");

        IEnumerable<String> entries = ReadMultiJsonFile($"SamplePayloads/{deviceName}.json");

        // read the environment variable REPEAT and set as false if null
        bool repeat = Environment.GetEnvironmentVariable("REPEAT") == null ? false : true;
        // Read the environment variable UPDATE_INTERVAL and set to 1 second if null
        int updateInterval = Environment.GetEnvironmentVariable("UPDATE_INTERVAL") == null ? 1000 : int.Parse(Environment.GetEnvironmentVariable("UPDATE_INTERVAL"));

        do
        {
            foreach (string entry in entries)
            {

                // Get the current time and serialize it in a string that can be stored in a json file
                string currentTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                
                // Replace the <replace> placeholder in the json file with the current time
                string updatedEntry = entry.Replace("<replace>", currentTime);
                
                Console.WriteLine($"Device '{deviceName}': Publishing {currentTime}");
                var puback = await mqttClient.PublishStringAsync($"{deviceName}.mqtt.contoso.com/vehiclestatus", updatedEntry);
                Console.WriteLine(puback.ReasonString);
                await Task.Delay(updateInterval);
            }
        } 
        while (repeat);


    }

    static IEnumerable<String> ReadMultiJsonFile(string filePath)
    {
        string fileContents = File.ReadAllText(filePath);
        IEnumerable<JObject> objects = JsonConvert.DeserializeObject<List<JObject>>(fileContents);
        return objects.Select(o => o.ToString());
    }
}
