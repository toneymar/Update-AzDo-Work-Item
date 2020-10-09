using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace UpdateTargetRelease
{
    class Program
    {
        private static async Task<string> GetJson(string accessToken, string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(
                            System.Text.ASCIIEncoding.ASCII.GetBytes(
                                string.Format("{0}:{1}", "", accessToken))));

                    using (HttpResponseMessage response = client.GetAsync(url).Result)
                    {
                        response.EnsureSuccessStatusCode();
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to get data from API using {url}");
                throw ex;
            }
        }
        private static async Task<string> PutJson(string accessToken, string url, string json)
        {
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(
                            System.Text.ASCIIEncoding.ASCII.GetBytes(
                                string.Format("{0}:{1}", "", accessToken))));

                    using (HttpResponseMessage response = client.PutAsync(url, content).Result)
                    {
                        response.EnsureSuccessStatusCode();
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to post data to API using {url}");
                throw ex;
            }
        }

        private static async Task AddOption(string accessToken, string requestUrl)
        {
            //Get json from api and deserialize it into an object
            string currentListJson = await GetJson(accessToken, requestUrl);
            ListObj currentList = JsonConvert.DeserializeObject<ListObj>(currentListJson);

            //Get the option to be added
            Console.WriteLine("Type the option you would like to add:");
            string addItem = Console.ReadLine();

            //Make sure a value was entered
            if (addItem == "")
            {
                Console.WriteLine("You must enter a value.\n");
                return;
            }

            //Add the option
            currentList.items.Add(addItem);

            //Re-serialize to json
            string updatedListJson = JsonConvert.SerializeObject(currentList);

            //Send the request
            await PutJson(accessToken, requestUrl, updatedListJson);
            Console.WriteLine("\nSuccessfuly updated the list in Azure DevOps.\n");
        }

        private static async Task RemoveOption(string accessToken, string requestUrl)
        {
            //Get json from api and deserialize it into an object
            string currentListJson = await GetJson(accessToken, requestUrl);
            ListObj currentList = JsonConvert.DeserializeObject<ListObj>(currentListJson);

            //Make sure the list isn't empty
            if (currentList.items.Count == 0)
            {
                Console.WriteLine("The list is empty.");
                return;
            }

            //List the items that are available to delete
            Console.WriteLine("Select an item to remove by typing a number, then pressing 'Enter':");
            for (int i = 0; i < currentList.items.Count; i++)
            {
                Console.WriteLine($"{i + 1} - {currentList.items[i]}");
            }

            //Grab the index of the item to delete
            string consoleRead = Console.ReadLine();
            if (consoleRead == "")
            {
                Console.WriteLine("You must enter a value.\n");
                return;
            }

            //Ensure a numerical value was entered
            try
            {
                Int32.Parse(consoleRead);
            }
            catch
            {
                Console.WriteLine("\nYou must enter a numerical value\n.");
                return;
            }

            int deleteIndex = Int32.Parse(consoleRead);

            //Delete the item
            try
            {
                currentList.items.RemoveAt(deleteIndex - 1);
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("\nThe number you entered was invalid.\n");
                return;
            }

            //Re-serialize to json
            string updatedListJson = JsonConvert.SerializeObject(currentList);

            //Send the request
            await PutJson(accessToken, requestUrl, updatedListJson);
            Console.WriteLine("\n\nSuccessfuly updated the list in Azure DevOps.\n");
        }

        private static async Task EditOption(string accessToken, string requestUrl)
        {
            //Get json from api and deserialize it into an object
            string currentListJson = await GetJson(accessToken, requestUrl);
            ListObj currentList = JsonConvert.DeserializeObject<ListObj>(currentListJson);

            //Make sure the list isn't empty
            if (currentList.items.Count == 0)
            {
                Console.WriteLine("The list is empty.");
                return;
            }

            //List the items that are available to edit
            Console.WriteLine("Select an item to edit by typing a number, then pressing 'Enter':");
            for (int i = 0; i < currentList.items.Count; i++)
            {
                Console.WriteLine($"{i + 1} - {currentList.items[i]}");
            }

            //Grab the index of the item to edit
            string consoleRead = Console.ReadLine();

            //Make sure a value was entered
            if (consoleRead == "")
            {
                Console.WriteLine("You must enter a value.\n");
                return;
            }

            //Make sure the value entered is a number
            try
            {
                Int32.Parse(consoleRead);
            }
            catch
            {
                Console.WriteLine("\nYou must enter a numerical value.\n");
                return;
            }

            int editIndex = Int32.Parse(consoleRead);

            //Grab the text to change it to
            try
            {
                Console.WriteLine($"\nEnter what you would like to change '{currentList.items[editIndex - 1]}' to:");
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("\nThe number you entered was out of range.\n");
                return;
            }
            string editedOption = Console.ReadLine();
           
            //Make sure a value was actually entered.
            if (editedOption == "")
            {
                Console.WriteLine("You must enter a value.\n");
                return;
            }

            //Make the change
            currentList.items[editIndex - 1] = editedOption;

            //Re-serialize to json
            string updatedListJson = JsonConvert.SerializeObject(currentList);

            //Send the request
            await PutJson(accessToken, requestUrl, updatedListJson);
            Console.WriteLine("\nSuccessfuly updated the list in Azure DevOps.\n");

        }

        private static async Task Menu(string accessToken, string requestUrl)
        {
            //Show the options menu
            Console.WriteLine("Select an option by typing a number, the pressing 'Enter':");
            string[] options = { "0 - Exit", "1 - Add an option", "2 - Edit an option", "3 - Remove an option" };
            foreach (string option in options)
            {
                Console.WriteLine(option);
            }

            //Get the user input
            switch (Console.ReadLine())
            {
                case "0":
                    break;
                case "1":
                    Console.WriteLine("");
                    await AddOption(accessToken, requestUrl);
                    await Menu(accessToken, requestUrl);
                    break;
                case "2":
                    Console.WriteLine("");
                    await EditOption(accessToken, requestUrl);
                    await Menu(accessToken, requestUrl);
                    break;
                case "3":
                    Console.WriteLine("");
                    await RemoveOption(accessToken, requestUrl);
                    await Menu(accessToken, requestUrl);
                    break;
                default:
                    Console.WriteLine("\nInvalid input\n");
                    await Menu(accessToken, requestUrl);
                    break;
            }
        }

        static async Task Main(string[] args)
        {
            //API request params
            string accessToken = "PLACEHOLDER";
            string requestUrl = "https://dev.azure.com/ViewpointVSO/_apis/work/processes/lists/2b075466-b4fa-4b5b-8013-f3bbd27c6a20?api-version=6.0-preview.1";

            //Get json from api and deserialize it into an object
            string currentListJson = await GetJson(accessToken, requestUrl);
            ListObj currentList = JsonConvert.DeserializeObject<ListObj>(currentListJson);

            //Print current options to the console
            Console.WriteLine("Current target releases:");
            
            if (currentList.items.Count == 0)
            {
                Console.WriteLine("The list is empty.");
            }

            for (int i = 0; i < currentList.items.Count; i++)
            {
                Console.WriteLine(currentList.items[i]);
            }
            Console.WriteLine("");

            await Menu(accessToken, requestUrl);
        }
    }
}
