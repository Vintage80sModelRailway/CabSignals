using JMRIReader;
using MQTTnet;
using Newtonsoft.Json;
using System.Buffers;
using System.Net;
using System.Text;

List<string> ProcessedTurnouts = new List<string>();
using var httpClient = new HttpClient();
var factory = new MqttClientFactory();
using var client = factory.CreateMqttClient();

// Track the last time a message arrived to handle the "unknown count"
DateTime lastMessageTime = DateTime.Now;
bool receivedAny = false;

List<string> processedTurnouts = new List<string>();

client.ApplicationMessageReceivedAsync += async e =>
{
    // Use .Payload and .ToArray() to avoid the "lacks get accessor" error
    byte[] bytes = e.ApplicationMessage.Payload.ToArray();
    string payload = Encoding.UTF8.GetString(bytes);
    string topic = e.ApplicationMessage.Topic;

    var topicSplit = topic.Split('/').ToList();
    var turnoutName = topicSplit.LastOrDefault();
    var decoder = Encoding.UTF8.GetDecoder();
    turnoutName = "MT" + turnoutName;

    if (!processedTurnouts.Contains(turnoutName))
    {
        try
        {
            var tresponse = await httpClient.GetAsync("http://192.168.1.29:8080/json/turnout/" + turnoutName);
            var success = tresponse.EnsureSuccessStatusCode();
            if (success.IsSuccessStatusCode)
            {
                var jsonResponse = await tresponse.Content.ReadAsStringAsync();
                var tobject = JsonConvert.DeserializeObject<TurnoutRootobject>(jsonResponse);
                if (tobject != null && tobject.data.state == 0)
                {
                    APIBaseTurnout to = new APIBaseTurnout();
                    to.state = payload == "THROWN" ? 4 : 2;


                    var json = JsonConvert.SerializeObject(to, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync("http://192.168.1.29:8080/json/turnout/" + turnoutName, content)
                                       .ConfigureAwait(false);

                    // 4. Check the result
                    if (response.IsSuccessStatusCode)
                    {
                        var rjResponse = await response.Content.ReadAsStringAsync();
                        var rjobject = JsonConvert.DeserializeObject<TurnoutRootobject>(rjResponse);
                        processedTurnouts.Add(turnoutName);
                        if (rjobject != null)
                        {
                            Console.WriteLine(turnoutName + " - " + rjobject.data.state);
                        }                        
                    }
                    else
                    {
                        Console.WriteLine($"API Failed: {response.StatusCode}");
                    }
                }
            }
        }
        catch (Exception ex) 
            {
            Console.WriteLine(ex.Message + " - "+turnoutName);
        }

    }

    // Reset the "silence" timer
    lastMessageTime = DateTime.Now;
    receivedAny = true;

};

var options = new MqttClientOptionsBuilder()
    .WithTcpServer("192.168.1.29")
    .Build();

try
{
    await client.ConnectAsync(options);
    await client.SubscribeAsync("track/turnout/#");

    Console.WriteLine("Connected to 192.168.1.29. Listening for track/turnout/#...");
    Console.WriteLine("(Script will exit after 10 seconds of silence)");

    // Keep running as long as messages keep coming (within 10s of each other)
    while (DateTime.Now - lastMessageTime < TimeSpan.FromSeconds(2))
    {
        await Task.Delay(1000);
    }

    await client.DisconnectAsync();

    if (receivedAny)
        Console.WriteLine("\nFinished: 10s of inactivity detected.");
    else
        Console.WriteLine("\nTimed out: No messages were received in the initial 10s window.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
