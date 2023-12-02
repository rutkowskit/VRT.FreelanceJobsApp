# VRT Freelance Jobs Explorer

This application explores available freelance jobs. <br/>

### Main goals
* It should allow freelancers to explore job opportunities.
* The design should accommodate the addition of more job providers in the future
* The application should maintain simplicity in its functionality.
* Application data should be stored in a plain JSON file.


![image info](./docs/pictures/App_MainView.png)

# VRT Freelance Jobs Explorer

## Introduction

**VRT Freelance Jobs Explorer** is an The application is designed to help freelancers find job opportunities 
from various job offer providers that have been configured in the application.
It provides a user-friendly interface to browse job offers in a unified way.

## Features

- **Job Exploration**: Browse through a wide range of jobs listed by the configured jobs providers in various categories. (eg. Useme).
- **Advanced Filtering**: Filter cached job by all available text values.
- **Hiding Unwanted Items**: Hide jobs that you're not interested in.

## Supported Jobs Providers

- **[Useme](https://useme.com)** 

## Building and Installation

1. Open Command Line
1. Clone the repository: `git clone https://github.com/rutkowskit/VRT.FreelanceJobsExplorer.git`
1. Navigate to the project directory: `cd VRT.FreelanceJobsExplorer`
1. Execute command `dotnet publish -c Release`
1. Copy all files from subfolder `.\VRT.FreelanceJobs.Wpf\bin\Release\net8.0-windows\publish\` to the installation folder.

## Configuration

In order to configure job offers provider:
1. Open the configuration file `appsettings.json` from the installation directory in a text editor (eg. `notepad++`),
1. Set Job Providers and Categories you want to explore. Example:
	```json
    {
      "Useme": {
        "BaseUri": "https://useme.com",
        "Categories": [ "programowanie-i-it,35", "prace-biurowe,40/bazy-danych,130" ]
      }
    }
	```
1. Save and close the file

## Usage
Go to the installation directory and execute `FreelanceJobs.exe` file.
Check available features in the next section of this document.

### Available toolbar options

#### Get New Jobs
![image info](./docs/pictures/GetNewJobsButton.png)
Use this option to receive all new job offers from configured providers

#### Update All Jobs
![image info](./docs/pictures/UpdateAllJobsButton.png)
Use this option to update cached Job offers with current data from configured providers

#### Toggle Visible/Hidden jobs
![image info](./docs/pictures/ToggleViewButton.png)
Use this option to toggle between visible and hidden job offers

#### Save job visibilities
![image info](./docs/pictures/SaveJobsButton.png)
Save the changed visibility of job offers. 
You should use this option after you toggle visibility of any job offer on the list.
It will store your changes in the cache.

### Job list filtering
You can filter jobs using a filter textbox.
![image info](./docs/pictures/FilterTextBox.png)
Insert the phrases that interest you, separated by spaces.
Example: 

`.net scrapper` will filter all jobs containing both `.net` and `scrapper` text in any field

String comparison is case insensitive.

## License

This project is licensed under the MIT License. See the LICENSE file for details.
