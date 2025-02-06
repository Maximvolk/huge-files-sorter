namespace Generator.Output
{
    public interface IOutputWriter : IAsyncDisposable
    {
        Task WriteLineAsync(string line);
    }
}
