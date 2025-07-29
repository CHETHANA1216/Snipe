# [Your Project Name]

## C# Playwright Automation for Snipe-IT Demo

This project provides C# Playwright automation scripts to interact with the Snipe-IT Asset for Global 360 .
It demonstrates functionalities such as logging in, searching for a specific asset, verifying its asset details, and checking the  asset's history by taking a screenshot.

Create Asset has not been implemented due to time constraints.
Due to this, the testing has to be carried out by  following the config steps mentioned in the 4th point.

1. Goto https://demo.snipeitapp.com/hardware after logging in
2. Find an asset that has the status ""Ready to deploy"" and that has a tag ""Deployed ""
3. Copy the serialnumber to assetTobeFound Variabe in the MainSnipeTest.cs
4. The screenshot of the final history tab can be viewed in the bin directory


## Setup

1.  **Clone the repository (or create your project):**
    If you haven't already, clone this repository to your local machine:
    ```bash
    git clone [https://github.com/your-username/your-repository-name.git](https://github.com/your-username/your-repository-name.git)
    cd your-repository-name
    ```
    If you're creating a new project, navigate into your project directory.

2.  **Install Playwright NuGet Package:**
    Open your terminal or command prompt in the root directory of your project (where your `.csproj` file is located) and run:
    ```bash
    dotnet add package Microsoft.Playwright
    ```

3.  **Install Playwright Browsers:**
    After adding the NuGet package, you need to install the browser binaries that Playwright uses.
    ```bash
    playwright install
    ```
    (This command assumes `playwright` is in your PATH, which it usually is after `dotnet add package`).
 4.  Due to this, the testing has to be carried out by  following the below steps.

    1. Goto https://demo.snipeitapp.com/hardware after logging in
    2. Find an asset that has the status ""Ready to deploy"" and that has a tag ""Deployed ""
    3. Copy the serialnumber to assetTobeFound Variabe in the MainSnipeTest.cs
    4. The screenshot of the final history tab can be viewed in the bin directory

