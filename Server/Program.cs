using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using Server;
using static Server.RequestMethodType;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
using System;

var port = 5000;
var server = new TcpListener(IPAddress.Loopback, port);

server.Start();
Console.WriteLine("Server started");



Category category = new Category(1, "Beverages");
Category category2 = new Category(2, "Condiments");
Category category3 = new Category(3, "Confections");

CategoriesList.categoriesList.Add(category);
CategoriesList.categoriesList.Add(category2);
CategoriesList.categoriesList.Add(category3);
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

    var request = JsonSerializer.Deserialize<Request>(requestText);

    
    // HandleRequest(stream, request);
    if (!string.IsNullOrEmpty(request.Path) && request.Path != "testing")
    {
        HandleApi(stream, request);
    }

    stream.Close();
}


static void HandleRequest(NetworkStream stream, Request request)
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


    if (string.IsNullOrEmpty(request?.Path) && Enum.TryParse<RequestMethodTypeEnum>(request?.Method, out RequestMethodTypeEnum result2))
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


    string[] pathToArr = new string[] { "" };
    //somewhere in your code
    pathToArr = request.Path.Split('/');

    if (pathToArr[1] != "api" || pathToArr[2] != "categories")
    {
        var test = new Response { Status = "4 Bad Request" };
        extendedResponse += "4 Bad Request";
        extendedBody = null;

        Response response = CreateResponse(extendedResponse, extendedBody);
        SendResponse(stream, response);
        return;

    }
    if (pathToArr.Length == 4)
    {
        if (pathToArr[1] != "api" || pathToArr[2] != "categories" || !Regex.IsMatch(pathToArr?[3], @"\d"))
        {
            var test = new Response { Status = "4 Bad Request" };
            extendedResponse += "4 Bad Request";
            extendedBody = null;
            Response response = CreateResponse(extendedResponse, extendedBody);
            SendResponse(stream, response);
            return;

        }
    }



    switch (request.Method)
    {
        case "create":
            handleCreateApi(stream, request);
            break;
        case "read":
            hadndleReadApi(stream, request);
            break;
        case "update":
            handleUpdateApi(stream, request);
            break;
        case "delete":
            handleDeleteApi(stream, request);
            break;
        case "echo":
            break;

    }


}

static void handleDeleteApi(NetworkStream stream, Request request)
{
    if (request.Path.Length <= 15)
    {
        Response response = CreateResponse("4 Bad Request", "");
        SendResponse(stream, response);
        return;
    }

}

static void hadndleReadApi(NetworkStream stream, Request request)
{
   


    string[] pathToArr = new string[] { "" };
    pathToArr = request.Path.Split('/');
    if (pathToArr.Length == 3)
    {
        readCategories(stream, request);
    }
    else if (pathToArr.Length == 4)
    {
        readCategory(stream, request, Convert.ToInt32(pathToArr[3]));

    }
}

static void readCategories(NetworkStream stream, Request request)
{
    string cat1ToJson = JsonSerializer.Serialize(CategoriesList.categoriesList);
    Response response = CreateResponse("1 Ok", cat1ToJson);
    SendResponse(stream, response);
    return;

}

static void readCategory(NetworkStream stream, Request request, int index)
{
    Category foundCategory = CategoriesList.categoriesList.FirstOrDefault(c => c.Id == index);

    if (foundCategory == null)
    {
        Response response2 = CreateResponse("5 not found", "");
        SendResponse(stream, response2);
        return;
    }


    string cat1ToJson = JsonSerializer.Serialize<Category>(foundCategory);
    Response response = CreateResponse("1 Ok", cat1ToJson);
    SendResponse(stream, response);
    return;

}

static void handleCreateApi(NetworkStream stream, Request request)
{

    var prefixToMatch = "/api/categories";
    string[] pathToArr = new string[] { "" };
    pathToArr = request.Path.Split('/');
    if(pathToArr.Length != 3)
    {
        Response response = CreateResponse("4 Bad Request", "");
        SendResponse(stream, response);
        return;
    }
    createCategory(stream, request);

}

static void createCategory(NetworkStream stream, Request request)
{

    // Deserialize JSON into a Category object
    var cat = JsonSerializer.Deserialize<Category>(request.Body, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true // To handle case-insensitive property names
    });

    var catObj = new Category(2, cat?.Name);
    string cat1ToJson = JsonSerializer.Serialize(catObj);
    Response response2 = CreateResponse("", cat1ToJson);
    SendResponse(stream, response2);

}

static void handleUpdateApi(NetworkStream stream, Request request)
{
    string[] pathToArr = new string[] { "" };
    pathToArr = request.Path.Split('/');
    if (pathToArr.Length != 4)
    {
        Response response = CreateResponse("4 Bad Request", "");
        SendResponse(stream, response);
        return;
    }
    updateCategory(stream, request, Convert.ToInt32(pathToArr[3]));
    
}

static void updateCategory(NetworkStream stream, Request request, int index)
{
    var req = JsonSerializer.Deserialize<Category>(request.Body, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true // To handle case-insensitive property names
    });

    Category foundCategory = CategoriesList.categoriesList.FirstOrDefault(c => c.Id == index);
    if (foundCategory == null )
    {

        Response response3 = CreateResponse("5 not found", "");
        SendResponse(stream, response3);
        return;

    }
    foundCategory.Name = req.Name;

    string cat1ToJson = JsonSerializer.Serialize<Category>(foundCategory);

    Console.WriteLine("UDATE {0} !", cat1ToJson);

    Response response2 = CreateResponse("3 updated", cat1ToJson);
    SendResponse(stream, response2);

}