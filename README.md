# Ed-Fi Learning Standards

The Ed-Fi Learning Standards command-line interface (CLI) is a tool geared
towards system administrators for validating connections, and synchronizing
learning standards between the AB Connect API and a specified Ed-Fi ODS
instance. Using the tool requires active credentials for both the target ED-Fi
ODS instance, and the AB Connect API.

## Installation

### Obtain Executables

Latest builds of the Ed-Fi Learning Standards command-line interface (CLI), and
Core SDK can be downloaded from the following location:

[Ed-Fi Tech Docs: Getting Started - Learning Standards Sync
Utility](https://techdocs.ed-fi.org/display/EDFITOOLS/Getting+Started+-+Learning+Standards+Sync+Utility)

### Extract to Location

Once downloaded, extract the archive file to the desired location, and navigate
to the folder. The file named `EdFi.Admin.LearningStandards.CLI.exe` is the CLI
application to use.

### Install AB Vendor Claimset to ODS

While in the extracted directory, navigate to the `/scripts` folder which
contains SQL install scripts for both V2, and V3 of the ODS database. Install
the AB Vendor Claimset by running either the `AB Claim Set v2.sql` or `AB Claim
Set v3.sql` script against the specific ODS implementation version being used.

:exclamation: Note: The CLI operator is then responsible for configuring the
Ed-Fi API Key and secret for the CLI with that new claimset.

### *Optional*: Add to PATH variable

Some may find it useful to add the CLI location to the system `PATH` variable so
the CLI can be used globally, without having to navigate to the install
location. If so, please follow instructions for doing so as outlined by your
operating system.

## Usage

### PATH Installation

If the CLI application was added to the system `PATH` variable during
installation, the following call can be used:

```PowerShell
EdFi.Admin.LearningStandards.CLI.exe <command> <required-arguments> [optional-arguments]
```

### Standalone Installation

If the CLI is being used in a standalone location, either call the
fully-qualified path of the tool, or navigate to the directory the CLI is being
kept in:

```PowerShell
> # Either:
> cd C:\Apps
> EdFi.Admin.LearningStandards.CLI.exe <command> <required-arguments> [optional-arguments]
> #
> # or
> C:\Apps\EdFi.Admin.LearningStandards.CLI.exe <command> <required-arguments> [optional-arguments]
```

### Commands

The Ed-Fi Learning Standards command-line interface (CLI) currently contains 3
commands that can be used with the respective options:

* `EdFi.Admin.LearningStandards.CLI.exe`
* `EdFi.Admin.LearningStandards.CLI.exe sync`
* `EdFi.Admin.LearningStandards.CLI.exe validate`
* `EdFi.Admin.LearningStandards.CLI.exe changes`

#### Options

`--version`\
Display the application version of the CLI.

`--help`\
Displays a list of available commands, and their descriptions. Use
`EdFi.Admin.LearningStandards.CLI.exe <command> --help` to display help context
specific to that command.

#### Examples

List available commands

```PowerShell
> EdFi.Admin.LearningStandards.CLI.exe --help
```

List help for the `sync` command

```PowerShell
> EdFi.Admin.LearningStandards.CLI.exe sync --help
```

Determine CLI application version

```PowerShell
> EdFi.Admin.LearningStandards.CLI.exe --version

  Ed-Fi Learning Standards CLI: 1.0.0
```

### Command: sync

The `EdFi.Admin.LearningStandards.CLI.exe sync` command provides the ability to
synchronize learning standards from the AB Connect API to a specified Ed-Fi ODS
instance.

```PowerShell
> EdFi.Admin.LearningStandards.CLI.exe sync [options]
```

#### sync Options

`--ab-connect-id` *Required*\
The Academic Benchmarks AB Connect ID to use.

`--ab-connect-key` *Required*\
The Academic Benchmarks AB Connect Key to use.

`--ed-fi-url` *Required*\
The Ed-Fi ODS url to use.

`--ed-fi-key` *Required*\
The Ed-Fi ODS API key to use.

`--ed-fi-secret` *Required*\
The Ed-Fi ODS API secret to use.

`--ab-auth-window`\
The buffer window, in seconds to use when refreshing an upcoming token
expiration. Defaults to 300.

`--ab-retry-limit`\
The number of retry attempts the application will make in case of failure.
Defaults to 3.

`--ed-fi-auth-url`\
The Ed-Fi ODS authentication url to use. Defaults to the oauth section of the
provided base url.

`--ed-fi-version`\
The Ed-Fi ODS version to use. Defaults to latest version.

`--ed-fi-school-year`\
The school year to use when querying the Ed-Fi ODS API.

`--ed-fi-retry-limit`\
The number of retry attempts the application will make in case of failure.
Defaults to 2.

`--ed-fi-simultaneous-request-limit`\
The number of simultaneous requests allowed during synchronization. Defaults to
4.

`--force-full`\
Instructs the CLI to force a full synchronization, ignoring the current change
state and replacing it.

`-v, --verbose`\
Set output to verbose messages.

`-u, --unattended`\
If enabled, the application will close immediately when finished.

`--help`\
Display this help screen for the sync method.

#### sync Examples

Basic usage

```PowerShell
> EdFi.Admin.LearningStandards.CLI.exe sync `
  --ab-connect-id test_account `
  --ab-connect-key ajk84Hjk93h59skaAJ8732 `
  --ed-fi-url https://api.ed-fi.org `
  --ed-fi-key RvcohKz9zHI4 `
  --ed-fi-secret E1iEFusaNf81xzCxwHfbolkC
```

### Command: validate

The `EdFi.Admin.LearningStandards.CLI.exe validate` command provides the ability
to validate the specified AB Connect API and Ed-Fi ODS instance configuration
settings.

```PowerShell
> EdFi.Admin.LearningStandards.CLI.exe validate [options]
```

#### validate Options

`--ab-connect-id` *Required*\
The Academic Benchmarks AB Connect ID to use.

`--ab-connect-key` *Required*\
The Academic Benchmarks AB Connect Key to use.

`--ed-fi-url` *Required*\
The Ed-Fi ODS url to use.

`--ed-fi-key` *Required*\
The Ed-Fi ODS API key to use.

`--ed-fi-secret` *Required*\
The Ed-Fi ODS API secret to use.

`--ab-auth-window`\
The buffer window, in seconds to use when refreshing an upcoming token
expiration. Defaults to 300.

`--ab-retry-limit`\
The number of retry attempts the application will make in case of failure.
Defaults to 3.

`--ab-simultaneous-request-limit`\
The number of simultaneous requests allowed during synchronization. Defaults to
10.

`--ed-fi-auth-url`\
The Ed-Fi ODS authentication url to use. Defaults to the oauth section of the
provided base url.

`--ed-fi-version`\
The Ed-Fi ODS version to use. Defaults to latest version.

`--ed-fi-school-year`\
The school year to use when querying the Ed-Fi ODS API.

`--ed-fi-retry-limit`\
The number of retry attempts the application will make in case of failure.
Defaults to 2.

`--ed-fi-simultaneous-request-limit`\
The number of simultaneous requests allowed during synchronization. Defaults to
4.

`-v, --verbose`\
Set output to verbose messages.

`-u, --unattended`\
If enabled, the application will close immediately when finished.

`--help`\
Display this help screen for the validate method.

#### validate Examples

Basic usage

```PowerShell
> EdFi.Admin.LearningStandards.CLI.exe validate `
    --ab-connect-id test_account `
    --ab-connect-key ajk84Hjk93h59skaAJ8732 `
    --ed-fi-url https://api.ed-fi.org `
    --ed-fi-key RvcohKz9zHI4 `
    --ed-fi-secret E1iEFusaNf81xzCxwHfbolkC
```

### Command: changes

The `EdFi.Admin.LearningStandards.CLI.exe changes` command provides methods for
checking to see if changes exist based on the last persisted sequence id, and to
retrieve the current change sequence id from the API.

```PowerShell
> EdFi.Admin.LearningStandards.CLI.exe changes [options]
```

#### changes Options

`--ab-connect-id` *Required*\
The Academic Benchmarks AB Connect ID to use.

`--ab-connect-key` *Required*\
The Academic Benchmarks AB Connect Key to use.

`--ed-fi-url` *Required*\
The Ed-Fi ODS url to use.

`--ed-fi-key` *Required*\
The Ed-Fi ODS API key to use.

`--ed-fi-secret` *Required*\
The Ed-Fi ODS API secret to use.

`--ab-auth-window`\
The buffer window, in seconds to use when refreshing an upcoming token
expiration. Defaults to 300.

`--ab-retry-limit`\
The number of retry attempts the application will make in case of failure.
Defaults to 3.

`--ab-simultaneous-request-limit`\
The number of simultaneous requests allowed during synchronization. Defaults to
10.

`--max-sequence-id-only`\
Instructs the CLI to retrieve only the max sequence id from the AB API, instead
of the full summary.

`-o, --output`\
Set the output format to either the default value of `text`, or `json`.

`-v, --verbose`\
Set output to verbose messages.

`-u, --unattended`\
If enabled, the application will close immediately when finished.

`--help`\
Display this help screen for the validate method.

#### changes Examples

Basic usage

```PowerShell
> EdFi.Admin.LearningStandards.CLI.exe changes `
  --ab-connect-id test_account `
  --ab-connect-key ajk84Hjk93h59skaAJ8732 `
  --ed-fi-url https://api.ed-fi.org `
  --ed-fi-key RvcohKz9zHI4 `
  --ed-fi-secret E1iEFusaNf81xzCxwHfbolkC

Changes available: True
Current Sequence Id: 0
Available Sequence Id: 988335
```

## Troubleshooting

The Ed-Fi Learning Standards command-line interface (CLI) contains robust
logging, capable of displaying both on-screen messaging, and file log messaging.
These can be used to provide more information on potential issues that may be
encountered when using the tool.

### Using the [-v|--verbose] Argument

Using the [-v|--verbose] Argument will instruct the application to output log
messages that are not normally written to the command window. These can include
HTTP connection logs, authentication success or failure details, detailed
synchronization messages, and others.

Example:

```PowerShell
> EdFi.Admin.LearningStandards.CLI.exe validate `
  --verbose `
  --ab-connect-id test_account `
  --ab-connect-key ajk84Hjk93h59skaAJ8732 `
  --ed-fi-url https://api.ed-fi.org `
  --ed-fi-key RvcohKz9zHI4 `
  --ed-fi-secret E1iEFusaNf81xzCxwHfbolkC

Verbose: Arguments parsed successfully
Verbose: Creating logging services
Verbose: Adding learning standard services
Validating configuration
Error: There was a problem retrieving an access token from
  https://api.ed-fi.org/oauth/token.
Finished. Press any key to exit.
```

### Using the log files

Log files are written to the same directory as the application. These logs
contain more technical information than the `--verbose` switch does about the
operational flow of the application.

Example:

```PowerShell
> EdFi.Admin.LearningStandards.CLI.exe validate `
  --ab-connect-id test_account `
  --ab-connect-key ajk84Hjk93h59skaAJ8732 `
  --ed-fi-url https://api.ed-fi.org `
  --ed-fi-key RvcohKz9zHI4 `
  --ed-fi-secret E1iEFusaNf81xzCxwHfbolkC

09:34:03:561 DEBUG [AcademicBenchmarksAuthTokenManager] Created token for test_account, expiring 1543934043
09:34:03:687 DEBUG [EdFiOdsApiv3AuthTokenManager] An existing access token was not found. Starting refresh
09:34:03:722 DEBUG [EdFiOdsApiv3AuthTokenManager] Sending access token request to https://api.ed-fi.org/oauth/token
09:34:04:012 ERROR [EdFiOdsApiv3AuthTokenManager] The access token request responded with a NotFound status.
```

## Support

If you need support while using the Ed-Fi Learning Standards command-line
interface (CLI), you can log an issue in the [Ed-Fi
Tracker](https://tracker.ed-fi.org/projects/EDFI)

## Legal Information

Copyright (c) 2021 Ed-Fi Alliance, LLC and contributors.

Licensed under the [Apache License, Version 2.0](LICENSE) (the "License").

Unless required by applicable law or agreed to in writing, software distributed
under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
CONDITIONS OF ANY KIND, either express or implied. See the License for the
specific language governing permissions and limitations under the License.

See [NOTICES](NOTICES.md) for additional copyright and license notifications.
