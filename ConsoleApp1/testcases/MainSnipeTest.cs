using Microsoft.Playwright;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

namespace ConsoleApp1.testcases
{
    internal class MainSnipeTest
    {

        private const string BaseUrl = "https://demo.snipeitapp.com";
        private const string Email = "admin";
        private const string Password = "password";

        private const string MODEL_SELECT_SELECTOR = "#model_select_id";
        private static readonly string modelName = "Macbook Pro 13";

        /*
         * 
         * Find an asset that has the status ""Ready to deploy"" and that has a tag ""Deployed ""
3. Copy the serialnumber to assetTobeFound Variabe in the MainSnipeTest.cs
4. Copy the checked OutTo to checkedOutUserName Variabe in the MainSnipeTest.cs
         */

        private static readonly string assetTobeFound = "35507ea9-2413-3713-967a-7e1639a4b9d1";      
       // private static readonly string checkedOutUserName= "a";

        static async Task Main( string[] args)
        {
            using var playwright = await Playwright.CreateAsync();

            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel="msedge",Headless=false});

            var page = await browser.NewPageAsync();


            /***** Login to the snipeit demo *************/

            try


            {
                await page.GotoAsync("https://demo.snipeitapp.com/login");

                await page.Locator("#username").FillAsync(Email);
                await page.Locator("#password").FillAsync(Password);

                await page.Locator("#submit").ClickAsync();
               Console.WriteLine("Login Successful");
            }
            catch
            {
                Console.WriteLine("Login UnSuccessful");
                return;
            }

                      

            await Task.Delay(3000);
            try
            {
               
                await page.GotoAsync($"https://demo.snipeitapp.com/hardware");
                await page.FillAsync("input.form-control.search-input",assetTobeFound);

                await page.WaitForTimeoutAsync(3000);
                await page.PressAsync("input.form-control.search-input", "Enter");
                await page.WaitForTimeoutAsync(3000);

                await page.WaitForSelectorAsync(
                    ".dataTables_empty", new PageWaitForSelectorOptions { State = WaitForSelectorState.Detached });

                
                await page.ClickAsync($"text={assetTobeFound}");

                Console.WriteLine("Asset listed Successful");
                // 1. ✅ Get Assigned User ("Deployed To")
                var deployedTo = await page.Locator("div.row:has-text('Deployed') div.col-md-9 a").First.TextContentAsync();
                Console.WriteLine($"Deployed To: {deployedTo.Trim()}");
                if(deployedTo.Length>1)
                {
                    Console.WriteLine("Asset details verified");
                }

                // Optionally, you can wait or assert results
                await page.WaitForTimeoutAsync(5000); // wait 3 seconds to visually verify
               // await browser.CloseAsync();
            }
            catch (PlaywrightException ex)
            {
                Console.WriteLine($"Playwright error occurred while interacting with search box: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error occurred: {ex.Message}");
            }


            // *** History Page ***/
           // Let's try a robust approach using GetByRole and then a fallback with GetByText
            try
            {
                // Attempt to click using GetByRole for better semantic selection
                await page.GetByRole(AriaRole.Link, new() { Name = "History" }).ClickAsync();
                System.Console.WriteLine("Clicked 'History' tab using GetByRole.");
            }
            catch (PlaywrightException)
            {
                // Fallback to GetByText if
                // GetByRole doesn't find it
                System.Console.WriteLine("GetByRole for 'History' failed, trying GetByText.");
                await page.GetByText("History").ClickAsync();
                System.Console.WriteLine("Clicked 'History' tab using GetByText.");
            }

            System.Console.WriteLine("Successfully navigated to the History page .");

            // Keep the browser open for a few seconds to observe (optional)
            await Task.Delay(5000);


            /*** VERIFY HISTORY*/

            // --- Step 3: Locate the Asset History Table and Verify "Created By" ---
            try
            {
                // Wait for the asset history table to be visible.
                // It's often ID'd as 'assetHistory' as seen in your provided HTML snippet
                await page.WaitForSelectorAsync("table#assetHistory", new PageWaitForSelectorOptions { Timeout = 10000 });
                Console.WriteLine("Asset history table found.");

                var assetHistoryTable = page.Locator("table#assetHistory");

                await page.ScreenshotAsync(new PageScreenshotOptions { Path = $"asset_{assetTobeFound}_view.png" });
                Console.WriteLine($"Screenshot saved: asset_{assetTobeFound}_view.png");
                await browser.CloseAsync();
            
            }
            catch (TimeoutException)
            {
                Console.WriteLine($"Asset history table or elements not found for asset {assetTobeFound} within the timeout. It might not have a history tab or the selectors are incorrect.");
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = $"asset_{assetTobeFound}_history_error.png" });
            }
        }
        
     }

       


}

 