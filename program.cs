using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

class Program
{
    static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8; // Ensure proper ASCII display

        // Grim Reaper Banner
        Console.WriteLine(@"
=+ Reaper += 
Private tool [BETA]
");

        string webhookUrl = GetWebhookUrl();

        if (!string.IsNullOrEmpty(webhookUrl))
        {
            Console.WriteLine("Successfully Loaded Webhook!");
        }

        while (true)
        {
            Console.WriteLine("\nSelect an option:");
            Console.WriteLine("[1] Send Message");
            Console.WriteLine("[2] Delete Webhook");
            Console.WriteLine("[3] Spam Webhook");
            Console.WriteLine("[4] Change Webhook Name");
            Console.WriteLine("[5] Exit");
            Console.Write("Enter your choice: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await SendWebhookMessage(webhookUrl);
                    break;
                case "2":
                    await DeleteWebhook(webhookUrl);
                    break;
                case "3":
                    await SpamWebhookMessages(webhookUrl);
                    break;
                case "4":
                    await ChangeWebhookName(webhookUrl);
                    break;
                case "5":
                    Console.WriteLine("Goodbye!");
                    return;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }

    static string GetWebhookUrl()
    {
        string filePath = "webhook.txt";

        if (File.Exists(filePath))
        {
            // Read the webhook URL from the file
            string webhookUrl = File.ReadAllText(filePath).Trim();
            if (!string.IsNullOrEmpty(webhookUrl))
            {
                return webhookUrl; // Return the webhook URL if it's valid
            }
            else
            {
                Console.WriteLine("Unable to load webhook from file. You will need to input one later.");
            }
        }

        // Ask for the webhook URL if not found or invalid
        Console.Write("Enter your Discord Webhook URL: ");
        string newWebhookUrl = Console.ReadLine();
        File.WriteAllText(filePath, newWebhookUrl);
        Console.WriteLine("Webhook URL saved to webhook.txt.");
        return newWebhookUrl;
    }

    static async Task SendWebhookMessage(string webhookUrl)
    {
        Console.Write("Enter your message: ");
        string message = Console.ReadLine();

        using (HttpClient client = new HttpClient())
        {
            var payload = new StringContent($"{{\"content\":\"{message}\"}}", Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(webhookUrl, payload);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Message sent successfully!");
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
            }
        }
    }

    static async Task DeleteWebhook(string webhookUrl)
    {
        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.DeleteAsync(webhookUrl);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Webhook deleted successfully!");
            }
            else
            {
                Console.WriteLine($"Error deleting webhook: {response.StatusCode}");
            }
        }
    }

    static async Task SpamWebhookMessages(string webhookUrl)
    {
        Console.Write("Enter your message: ");
        string message = Console.ReadLine();

        Console.Write("Enter number of times to send message: ");
        if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
        {
            Console.WriteLine("Invalid input. Please enter a positive number.");
            return;
        }

        using (HttpClient client = new HttpClient())
        {
            for (int i = 0; i < count; i++)
            {
                var payload = new StringContent($"{{\"content\":\"{message}\"}}", Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(webhookUrl, payload);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[{i + 1}/{count}] Message sent successfully!");
                }
                else
                {
                    Console.WriteLine($"[{i + 1}/{count}] Error: {response.StatusCode}");
                    break;
                }

                await Task.Delay(540); // 1-second delay to avoid rate limits
            }
        }
    }

    static async Task ChangeWebhookName(string webhookUrl)
    {
        Console.Write("Enter the new name for the webhook: ");
        string newName = Console.ReadLine();

        using (HttpClient client = new HttpClient())
        {
            // Fetch the current webhook data using GET
            HttpResponseMessage response = await client.GetAsync(webhookUrl);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error fetching webhook: {response.StatusCode}");
                return;
            }

            string content = await response.Content.ReadAsStringAsync();
            dynamic webhookData = JsonConvert.DeserializeObject(content);

            // Update the webhook name
            webhookData.name = newName;

            // Send the updated data with a PATCH request
            var patchContent = new StringContent(JsonConvert.SerializeObject(webhookData), Encoding.UTF8, "application/json");
            HttpResponseMessage patchResponse = await client.PatchAsync(webhookUrl, patchContent);

            if (patchResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("Webhook name changed successfully!");
            }
            else
            {
                Console.WriteLine($"Error changing webhook name: {patchResponse.StatusCode}");
            }
        }
    }
}
