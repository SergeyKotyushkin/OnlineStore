namespace OnlineStore.BuisnessLogic.JsonSerialize.Contracts
{
    public interface IJsonSerializer
    {
        string Serialize(object inputObject);

        T Deserialize<T>(string inputString);
    }
}