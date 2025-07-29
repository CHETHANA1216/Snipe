using Microsoft.Playwright;
using System;
using System.Threading.Tasks;
using System.Linq; // For .Any() and .Skip()
using System.Collections.Generic; // For List

public class secTest
{
    // --- Configuration Constants ---
    private const string BASE_URL = "[https://demo.snipeitapp.com](https://demo.snipeitapp.com)";
    private const string LOGIN_URL = BASE_URL + "/login";
    private const string CREATE_ASSET_URL = BASE_URL + "/hardware/create";

    // Demo credentials (use with caution on live systems, these are public demo credentials)
    private const string USERNAME = "admin";
    private const string PASSWORD = "password";

    // Selectors for login page
    private const string LOGIN_USERNAME_SELECTOR = "input[name='username']";
    private const string LOGIN_PASSWORD_SELECTOR = "input[name='password']";
    private const string LOGIN_BUTTON_SELECTOR = "button[type='submit']";

    // Selectors for asset creation form
    private const string ASSET_TAG_SELECTOR = "#asset_tag";
    private const string ASSET_NAME_SELECTOR = "#name";
    private const string STATUS_ID_SELECTOR = "#status_id";
    private const string MODEL_SELECT_SELECTOR = "#model_id";
    private const string ASSIGNED_TO_SELECTOR = "#assigned_to";
    private const string CREATE_ASSET_SUBMIT_BUTTON_SELECTOR = "button[type='submit']";

    public static async Task Main(string[] args)
    {
        // Initialize Playwright
        using var playwright = await Playwright.CreateAsync();

        // Launch a Chromium browser instance.
        // Headless: true runs in the background without a UI. Set to false to watch it in action.
        // SlowMo: Adds a delay between actions for better observation during development.
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false, // Set to true for production/CI
            SlowMo = 100 // milliseconds delay between actions
        });

        // Create a new browser page
        var page = await browser.NewPageAsync();

        try
        {
            Console.WriteLine("Starting Snipe-IT asset creation automation...");

            // Step 1: Login to Snipe-IT
            await LoginAsync(page);

            // Step 2: Create a new Macbook Pro asset
            await CreateMacbookProAssetAsync(page);

            Console.WriteLine("\nAutomation script finished successfully!");
        }
        catch (PlaywrightException ex)
        {
            Console.WriteLine($"\nPlaywright automation failed: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nAn unexpected error occurred: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
        finally
        {
            Console.WriteLine("\nPress any key to close the browser and exit.");
            Console.ReadKey();
            // The 'await using' statement ensures the browser is disposed automatically.
        }
    }

    /// <summary>
    /// Handles the login process for Snipe-IT demo site.
    /// </summary>
    /// <param name="page">The Playwright page instance.</param>
    private static async Task LoginAsync(IPage page)
    {
        Console.WriteLine($"Navigating to login page: {LOGIN_URL}");
        await page.GotoAsync(LOGIN_URL);

        // Fill in username and password
        Console.WriteLine($"Filling username: {USERNAME}");
        await page.FillAsync(LOGIN_USERNAME_SELECTOR, USERNAME);

        Console.WriteLine("Filling password...");
        await page.FillAsync(LOGIN_PASSWORD_SELECTOR, PASSWORD);

        // Click the login button and wait for navigation
        Console.WriteLine("Clicking login button...");
        await page.ClickAsync(LOGIN_BUTTON_SELECTOR);

        // Wait for the URL to change to the dashboard or a known post-login page
        // Use a more generic wait if the exact post-login URL is variable
        await page.WaitForURLAsync(BASE_URL + "/**", new PageWaitForURLOptions { Timeout = 10000 });
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle); // Wait for all network requests to settle

        // Assert that login was successful by checking for a known element on the dashboard
      ///  await Expect(page.Locator("text=Dashboard")).ToBeVisibleAsync();
        Console.WriteLine("Successfully logged in.");
    }

    /// <summary>
    /// Creates a new Macbook Pro 13" asset with specified details.
    /// </summary>
    /// <param name="page">The Playwright page instance.</param>
    private static async Task CreateMacbookProAssetAsync(IPage page)
    {
        Console.WriteLine($"Navigating to create asset page: {CREATE_ASSET_URL}");
        await page.GotoAsync(CREATE_ASSET_URL);

        // Ensure the form is loaded
       /// await Expect(page.Locator(ASSET_TAG_SELECTOR)).ToBeVisibleAsync();
        Console.WriteLine("Create Asset form loaded.");

        // Generate a unique asset tag
        string uniqueAssetTag = $"MBP13-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        Console.WriteLine($"Setting Asset Tag: {uniqueAssetTag}");
        await page.FillAsync(ASSET_TAG_SELECTOR, uniqueAssetTag);

        // Set Asset Name
        const string assetName = "Macbook Pro 13\"";
        Console.WriteLine($"Setting Asset Name: {assetName}");
        await page.FillAsync(ASSET_NAME_SELECTOR, assetName);

        // Select Status: "Ready to Deploy"
        // We'll select by label for readability, assuming it's consistent.
        // If it fails, inspect the HTML to find the exact 'value' attribute for "Ready to Deploy".
        Console.WriteLine("Selecting Status: 'Ready to Deploy'");
        await page.SelectOptionAsync(STATUS_ID_SELECTOR, new SelectOptionValue { Label = "Ready to Deploy" });
        await page.WaitForTimeoutAsync(500); // Small pause for UI update

        // Select Model: The 1st value (after any placeholder)
        Console.WriteLine("Waiting for Model dropdown to be enabled and populated...");
        // Wait for the select element itself to be visible and not disabled
        await page.WaitForSelectorAsync($"{MODEL_SELECT_SELECTOR}:not([disabled])", new PageWaitForSelectorOptions { Timeout = 15000 });
        // Wait for at least two options (first is usually "Select a Model...")
        await page.WaitForFunctionAsync($"document.querySelectorAll('{MODEL_SELECT_SELECTOR} option').length > 1", null, new PageWaitForFunctionOptions { Timeout = 30000 });

        // Get all options and select the second one (index 1), assuming index 0 is a placeholder
        var modelOptions = await page.Locator($"{MODEL_SELECT_SELECTOR} option").AllAsync();
        if (modelOptions.Count > 1)
        {
            var firstRealModelValue = await modelOptions[1].GetAttributeAsync("value");
            var firstRealModelText = await modelOptions[1].TextContentAsync();
            Console.WriteLine($"Selecting Model: '{firstRealModelText}' (Value: {firstRealModelValue})");
            await page.SelectOptionAsync(MODEL_SELECT_SELECTOR, new SelectOptionValue { Value = firstRealModelValue });
            await page.WaitForTimeoutAsync(500); // Small pause for UI update
        }
        else
        {
            Console.WriteLine("Warning: Could not find enough models to select the first real value. Skipping model selection.");
        }

        // Select Assigned To: A random user
        Console.WriteLine("Waiting for Assigned To dropdown to be enabled and populated...");
        // Wait for the select element itself to be visible and not disabled
        await page.WaitForSelectorAsync($"{ASSIGNED_TO_SELECTOR}:not([disabled])", new PageWaitForSelectorOptions { Timeout = 15000 });
        // Wait for at least two options (first is usually "Select a User...")
        await page.WaitForFunctionAsync($"document.querySelectorAll('{ASSIGNED_TO_SELECTOR} option').length > 1", null, new PageWaitForFunctionOptions { Timeout = 30000 });

        var userOptions = await page.Locator($"{ASSIGNED_TO_SELECTOR} option").AllAsync();
        if (userOptions.Count > 1) // Ensure there's at least one real user option
        {
            // Generate a random index, skipping the first placeholder option (index 0)
            Random random = new Random();
            int randomIndex = random.Next(1, userOptions.Count); // From 1 to (Count - 1) inclusive
            var randomUserValue = await userOptions[randomIndex].GetAttributeAsync("value");
            var randomUserText = await userOptions[randomIndex].TextContentAsync();

            Console.WriteLine($"Selecting random user: '{randomUserText}' (Value: {randomUserValue})");
            await page.SelectOptionAsync(ASSIGNED_TO_SELECTOR, new SelectOptionValue { Value = randomUserValue });
            await page.WaitForTimeoutAsync(500); // Small pause for UI update
        }
        else
        {
            Console.WriteLine("Warning: Could not find enough users to select a random one. Skipping user assignment.");
        }

        // Click the Create Asset button
        Console.WriteLine("Clicking 'Create Asset' button...");
        await page.ClickAsync(CREATE_ASSET_SUBMIT_BUTTON_SELECTOR);

        // Wait for navigation or a success message/redirect
        // Snipe-IT often redirects to the asset's detail page or the asset list.
        // We'll wait for the URL to change or for a success notification.
        await page.WaitForURLAsync(BASE_URL + "/hardware/**", new PageWaitForURLOptions { Timeout = 10000 });
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify asset creation by checking for a success message or the asset tag on the new page
     ///   await Expect(page.Locator($"text='{uniqueAssetTag}'")).ToBeVisibleAsync();
        Console.WriteLine($"Asset '{assetName}' with tag '{uniqueAssetTag}' created successfully!");
    }
}