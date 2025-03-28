namespace CommandService.EventProcessing
{
    public interface IEventProcessor
    {
        public void ProcessEvent(string messege);
    }
}
