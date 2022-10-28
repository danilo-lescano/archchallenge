using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace LambdaBalanceAPI;

public class BalanceProvider
{
    private readonly IAmazonDynamoDB dynamoDB;

    public BalanceProvider(IAmazonDynamoDB dynamoDB)
    {
        this.dynamoDB = dynamoDB;
    }

    public async Task<List<Balance>> GetBalanceListAsync()
    {
        var result = await dynamoDB.ScanAsync(new ScanRequest
        {
            TableName = "balance-table"
        });

        if (result != null && result.Items != null)
        {
            List<Balance> balanceList = new List<Balance>();

            foreach (var item in result.Items)
            {
                item.TryGetValue("userId", out var userId);
                item.TryGetValue("balanceValue", out var balanceValue);

                balanceList.Add(new Balance
                {
                    UserId = int.Parse(userId?.N),
                    BalanceValue = decimal.Parse(balanceValue?.S)
                });
            }
            return balanceList;
        }
        return new List<Balance>();
    }
}