namespace Generator
{
    public interface IProgressObserver
    {
        void ObserveProgress(double percent);
        void Finish();
    }
}