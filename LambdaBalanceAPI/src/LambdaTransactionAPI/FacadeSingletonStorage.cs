using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace LambdaBalanceAPI;

public class FacadeSingletonStorage
{
    private static FacadeSingletonStorage _instance;
    public static FacadeSingletonStorage Instance
    {
        get
        {
            if (_instance == null)
                _instance = new FacadeSingletonStorage();
            return _instance;
        }
    }
    protected FacadeSingletonStorage() { }

    private List<Balance> _balances;
    private async Task InstatiateBalance()
    {
        BalanceProvider balanceProvider = new BalanceProvider(new AmazonDynamoDBClient());
        _balances = await balanceProvider.GetBalanceListAsync();
    }

    public async Task<List<Balance>> GetBalances()
    {
        if (_balances == null)
            await InstatiateBalance();
        return _balances;
    }

    public async Task<Balance> GetBalanceById(int userId)
    {
        if (_balances == null)
            await InstatiateBalance();
        foreach (Balance _item in _balances)
        {
            if (_item.UserId == userId)
                return _item;
        }

        Balance item = new Balance
        {
            UserId = userId,
            BalanceValue = 0
        };
        _balances.Add(item);
        return item;
    }

    public async Task<bool> UpdateBalanceById(int userId, decimal balanceValue)
    {
        Balance item = await GetBalanceById(userId);
        item.BalanceValue += balanceValue;

        IAmazonDynamoDB dynamoDB = new AmazonDynamoDBClient();
        var response = await dynamoDB.PutItemAsync(new PutItemRequest
        {
            TableName = "balance-table",
            Item = new Dictionary<string, AttributeValue> {
                {"userId", new AttributeValue{N = item.UserId.ToString()}},
                {"balanceValue", new AttributeValue(item.BalanceValue.ToString())}
            }
        });
        return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
    }
}