using AutoMapper;
using CommandService.Data;
using CommandService.Dtos;
using CommandService.Models;
using System.Text.Json;

namespace CommandService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMapper _mapper;

        public EventProcessor(IServiceScopeFactory scopeFactory, IMapper mapper)
        {
            _scopeFactory = scopeFactory;
            _mapper = mapper;
        }
        public void ProcessEvent(string messege)
        {
            var eventType = DeterminateEvent(messege);
            switch (eventType)
            {
                case EventType.PlatformPublished:
                    AddPlatform(messege);
                    break;
                default:
                    break;
            }
        }

        private EventType DeterminateEvent(string notificationMessege)
        {
            Console.WriteLine("--> Determining Event");
            
            var eventType = JsonSerializer.Deserialize<GenericEventDto>(notificationMessege);

            switch (eventType.Event)
            {
                case "Platform_Published":
                    Console.WriteLine("Platform Published Event Detected");
                    return EventType.PlatformPublished;
                default:
                    Console.WriteLine("--> Could not determine the event type");
                    return EventType.Undetermined;
            }
        }

        private void AddPlatform(string platformPublishedMessege)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<ICommandRepo>();


                var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessege);

                try
                {
                    var plat = _mapper.Map<Platform>(platformPublishedDto);
                    if (!repository.ExternalPlatformExists(plat.ExternalID))
                    {
                        repository.CreatePlatform(plat);
                        repository.SaveChanges();
                        Console.WriteLine("-->Platform Added InMem ...");

                    }
                    else
                    {
                        Console.WriteLine("--> Platform already exists...");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Could not add Platform to DB {ex.Message}");
                }
            }
        }
    }
    enum EventType
    {
        PlatformPublished,
        Undetermined
    }
}
