﻿using PlatformService.Dtos;
using System.Text;
using System.Text.Json;

namespace PlatformService.SyncDataServices.Http
{
    public class HttpCommandDataClient : ICommandDataClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public HttpCommandDataClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }
        public async Task SendPlatformToCommand(PlatformReadDto plat)
        {
            var httpContent = new StringContent(
                    JsonSerializer.Serialize(plat),
                    Encoding.UTF8,
                    "application/json"
                );
            var response = await _httpClient.PostAsync($"{_configuration["CommandService"]}", httpContent);
            Console.WriteLine(_configuration["CommandService"]);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("--> Sync POST to CommandService Ok");
            } else
            {
                Console.WriteLine("--> Sync POST to CommandService NOT Ok");
            }
        }
    }
}
