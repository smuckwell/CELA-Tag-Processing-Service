function GetDeployment($requestedConfiguration, $configurations)
{
    #Iterate over the loaded configurations
    foreach ($configuration in $configurations.psobject.Properties)
    {
        #Return the configuration that matches the request
        if ($requestedConfiguration -eq $configuration.Value.ConfigurationDeploymentType)
        {
            return $configuration.Value
        }
    }
    $error = [string]::Concat('Could not find configuration of type "', $requestedConfiguration,'"')
    Write-Error $error
    return $null
}

function ProcessStringReplacementsIntoFiles($files, $currentDeployment, $requestedDeployment, $verbose = $false)
{
    #Set the filepath tokens to be excluded
    $filepathExclusions = @('CELA-Tag_Parsing_Service_Configuration_Utilities','.git','.vs','\bin','\obj','CELA_Tag_Services_Deployment_Configurations.json','CELA-Tagulous_Parsing_Service-Utils.ps1', 'HomeController.cs', 'TestRequest', 'TestEmails')
    $configurationPropertyTypeExclusions = @('ConfigurationDeploymentType','ConfigurationDeploymentTypeDescription')

    foreach ($file in $files)
    {
        if ($verbose)
        {
            [string]::Concat('Processing ', $file.PSPath,'.')
        }
        
        #Do not process files that should be excluded
        if ($null -eq ($filepathExclusions | ? { $file.PSPath -match [Regex]::Escape($_) }))
        {
            #[string]::Concat('Would change ', $file.PSPath,'.')
            
            $content = Get-Content $file.PSPath
            #Only process files that have content
            if (![string]::IsNullOrEmpty($content))
            {
                $fileUpdatesAvailable = $false
                #Iterate over each property in the deployment descriptors
                foreach ($property in $currentDeployment.psobject.Properties)
                {
                    #Skip the properties that are metadata, and not values to be replaced
                    if ($null -eq ($configurationPropertyTypeExclusions | ? { $property.Name -match $_ }))
                    {
                        #Get the old property values (to find) and new property values (to be replaced)
                        $oldValue = $currentDeployment | Select -ExpandProperty $property.Name
                        $newValue = $requestedDeployment | Select -ExpandProperty $property.Name
                        $match = $content -match [Regex]::Escape($oldValue)
                        if (![string]::IsNullOrEmpty($match))
                        {
                            if ($verbose)
                            {
                                [string]::Concat("`tFrom: ", $oldValue)
                                [string]::Concat("`tTo: ", $newValue)
                                [string]::Concat("`tContext: ",$match)
                                ''                                
                            }
                            $content = $content.replace($oldValue, $newValue)
                            $fileUpdatesAvailable = $true
                        }
                    }
                }
                #Only update the file if changes are available
                if ($fileUpdatesAvailable)
                {
                    [string]::Concat("Updating file: ", $file.PSPath)
                    $content | Out-File -filepath $file.PSPath
                }
            }
        }
    }
}

$fileTypesToUpdate = @('*.bot','*.config','*.cs','*.json','*.resx')

'Welcome to CELA Tagulous Parsing Service Utilities.'
[string]::Concat('This application will update the files of type ', $fileTypesToUpdate,' for the specified deployment type.')

#Note that you must customize your own copy of CELA_Tag_Services_Deployment_Configurations_(template).json
$deploymentConfigurationFileName = 'CELA_Tag_Services_Deployment_Configurations.json' 
$deploymentConfigurationFileNameDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$deploymentConfigurationSearchDirectory = Split-Path -Path $deploymentConfigurationFileNameDirectory -Parent
$deploymentConfigurationFilePath = Join-Path $deploymentConfigurationFileNameDirectory $deploymentConfigurationFileName
$deploymentConfigurations = Get-Content $deploymentConfigurationFilePath | ConvertFrom-Json

'Loaded configurations include:'
"`tType`tDescription"

foreach ($property in $deploymentConfigurations.psobject.Properties)
{
    [string]::Concat("`t",$property.Value.ConfigurationDeploymentType, "`t", $property.Value.ConfigurationDeploymentTypeDescription)
}

$deploymentConfigurationTypeCurrent = Read-Host -Prompt 'Please enter the current configuration type of your project (current state)'
$deploymentConfigurationTypeRequested = Read-Host -Prompt 'Please enter the configuration type you want to apply to your project (desired future state)'

$deploymentConfigurationCurrent = GetDeployment $deploymentConfigurationTypeCurrent $deploymentConfigurations
$deploymentConfigurationRequested = GetDeployment $deploymentConfigurationTypeRequested $deploymentConfigurations

if ($deploymentConfigurationCurrent -ne $null -and $deploymentConfigurationRequested -ne $null)
{
    $files = Get-ChildItem -Path $deploymentConfigurationSearchDirectory -Recurse -Include $fileTypesToUpdate
    ProcessStringReplacementsIntoFiles $files $deploymentConfigurationCurrent $deploymentConfigurationRequested    
}

Write-Warning 'You should clean your solution after running this script.'