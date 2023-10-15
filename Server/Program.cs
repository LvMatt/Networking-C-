using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using Server;
using static Server.RequestMethodType;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

var port = 5000;
var server = new TcpListener(IPAddress.Loopback, port);

server.Start();
Console.WriteLine("Server started");

while (true)
{
    var client = server.AcceptTcpClient();
    Console.WriteLine("Client connected...");

    try
    {
        HandleClient(client);
    }
    catch (Exception) {
        Console.WriteLine("Unable to communicate");
    }

}

static void SendResponse(NetworkStream stream, Response response)
{
    var responseText = JsonSerializer.Serialize<Response>(response);

    Console.WriteLine("Send Reponse, {0}", responseText);

    var responseBuffer = Encoding.UTF8.GetBytes(responseText);

    Console.WriteLine("Send responseBuffer, {0}", responseBuffer);


    stream.Write(responseBuffer);
}

static Response CreateResponse(string status, string body)
{
    if(body == "")
    {
        body = null;
    }
    Console.WriteLine("BODY!!, {0}", body);
    return new Response
    {
        Status = status,
        Body = body
    };
}

static void HandleClient(TcpClient client)
{
    var stream = client.GetStream();
    var buffer = new byte[1024];
    var rcnt = stream.Read(buffer);

    var requestText = Encoding.UTF8.GetString(buffer, 0, rcnt);

    Console.WriteLine("requestText33, {0}", requestText);

    var request = JsonSerializer.Deserialize<Request>(requestText);

    Console.WriteLine("Request data--", request);
   
        //HandleRequestMethod(stream, request);
   
        HandleApi(stream, request);

    stream.Close();
}


static void HandleRequestMethod(NetworkStream stream, Request request)
{
    var extendedResponse = "";
    string extendedBody = "";


    if (string.IsNullOrEmpty(request?.Method))
    {
        extendedResponse += "- 4 Missing methods -";
    }

    if (!Enum.TryParse<RequestMethodTypeEnum>(request?.Method, out RequestMethodTypeEnum result))
    {
        extendedResponse += "- illegal method - ";
    }


    if (string.IsNullOrEmpty(request?.Body))
    {
        extendedResponse += "- missing resource- ";
    }
    else
    {
        if ((request?.Body?.Trim().StartsWith("{") != true) && request.Method == "update" )
        {
            extendedResponse += "- illegal body- ";
        }
    }

    if (string.IsNullOrEmpty(request.Date))
    {
        extendedResponse += "- missing date- ";
    }

    if (request?.Date is string)
    {
        extendedResponse += "- illegal date- ";
    }

    if (string.IsNullOrEmpty(request.Body) && request?.Method == "create" || request?.Method == "update" || request.Method == "echo")
    {
        extendedResponse += "- missing body- ";
    }


    if (request?.Method == "echo" && !string.IsNullOrEmpty(request?.Body)) 
    {
        extendedBody = request.Body;
    }

    // extendedResponse = HandlePath(request, stream,extendedResponse, extendedBody);

    Response response = CreateResponse(extendedResponse, extendedBody);
    SendResponse(stream, response);


}

static void  HandleApi(NetworkStream stream, Request request)
{
    var extendedResponse = "";
    string extendedBody = null;
    var prefixToMatch = "/api/categories";
    if (!string.IsNullOrEmpty(request.Path) && request.Path != "testing")
    {

        string[] pathToArr = new string[] { "" };
        //somewhere in your code
        pathToArr = request.Path.Split('/');

        Console.WriteLine("DSAD, {0} {1} {2}", pathToArr[1], pathToArr[2], pathToArr.Length);
        if (pathToArr[1] != "api" || pathToArr[2] != "categories" )
        {
            var test = new Response { Status = "4 Bad Request" };
            extendedResponse += "4 Bad Request";
            extendedBody = null;

        }
        if(pathToArr.Length == 4)
        {
            if (pathToArr[1] != "api" || pathToArr[2] != "categories" || !Regex.IsMatch(pathToArr?[3], @"\d"))
            {
                var test = new Response { Status = "4 Bad Request" };
                extendedResponse += "4 Bad Request";
                extendedBody = null;

            }
        }

       
    }
    Response response = CreateResponse(extendedResponse, extendedBody);
    SendResponse(stream, response);
}
