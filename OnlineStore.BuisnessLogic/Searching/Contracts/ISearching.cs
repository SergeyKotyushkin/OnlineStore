namespace OnlineStore.BuisnessLogic.Searching.Contracts
{
    public interface ISearching<TData>
    {
        TData[] Search(TData[] data, string name, string category);
    }
}