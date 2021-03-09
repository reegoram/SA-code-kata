# SA-code-kata
Code kata

## Requirements
- .NET Core 3.1
- NodeJS v14.x

## Default configurations
The API runs on ports 5000 (HTTP) and 5001 (HTTPS), meanwhile the Client App runs on port 4200. In case of changing the ports, the configuration must get update in 

[appsettings.json](./backend/API/appsettings.json)
```json
{
  "AllowedOrigin": "" // Set here the client app url i.e. http://localhost:4200
}
```

and

[environment.ts](./frontend/src/environments/environment.ts)
```ts
export const environment = {
    ...
    apiUrl: ''; // Set here the API url. e.g. https://localhost:5001/api/
};
```

## Extras
A file with valid information is provided in [./valid-input-file.txt](./valid.input-file.txt)

Backend project is separated in layers following clean and hexagonal architecture. The core entities are domain objects, in the upper layer (application) there are some business logic, and in the outest layer there are implementation for infrastructure and API.

Unit testing was implemented with NUnit, Moq and FluentAssertion libraries. For testing, mocking and asserting respectively.

Client App is built with Angular v10 and some icons and fonts for design.
