using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using System.Threading.Tasks;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaTransactionAPI;

public class LambdaTransactionAPIFunction
{

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        int userId = 0;
        decimal balanceValue = 0;

        if (request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey("userId") && request.QueryStringParameters.ContainsKey("balanceValue"))
        {
            userId = int.Parse(request.QueryStringParameters["userId"]);
            balanceValue = decimal.Parse(request.QueryStringParameters["balanceValue"]);
        }

        context.Logger.Log($"userId:{userId} value:{balanceValue}");

        if (userId == 0 || balanceValue == 0)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "invalid value parameter"
            };
        }


        FacadeSingletonStorage instance = FacadeSingletonStorage.Instance;
        bool operation = await instance.UpdateBalanceById(userId, balanceValue);

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = operation ? "operation success" : "operation fail"
        };
    }
}