This is a sample solution demonstrating secure authentication from Blazor WASM with an AWS Serverless Lambda
application, authenticated by Cognito with api gateway authorization handled by a Lambda. In its fully deployed
state the Blazor client will run from an S3 static bucket, published by Cloud front. The application API runs as 
a Lambda (based on the AWS Serverless template). Authentication via Cognito has been implemented and the application
API reads the JWT from the client and uses that for identity. Connectivity from the Blazor client to the application 
API is routed through an AWS API gateway. The Gateway preauthenticates the request using the ApiGatewayAuthorizer Lambda
which is based on the AWS Lambda template. The preauthentication handles the "heavy" part of authentication, which is
the signature validation and issuer validation, that way the application API only needs to decode the JWT claims. 

The solution breaks down into the following modules:
BalsamicSolutions.ApiCommon	: This assembly contains the interfaces and objects shared between the Client and the Server
BalsamicSolutions.ApiSupportLambdas : This is a multifunction Lambda, you can register it with AWS API Gateway as the Lambda
				      module that handles request preauthentication, you can also register it to recive S3 events
				      to update the CloudFront publications. All configuration for this is stored in Lambda environment variables
BalsamicSolutions.ApiServer : This is the "Serverless" Lambda that hosts the controllers application API Endpoints
								 configuration for this is stored both the appsettings.json file and the Lambda environment variables
BalsamicSolutions.BlazorClient : This is the WASM Blazor client which is delivered from Cloud front
								  all configuration for this is stored in the appsettings.json file

Solution deployment is multi step, you have to deploy the "Serverless" application separatly from the authorizer and the 
cloud front distribution. That will be improved later

Authorization in the ApiServer uses standard "Authorize" attributes. We have implmented a sample authroization policy named
"InWeatherCenter" as an example of how to use this. While this demo does not use Dynamo or S3, those services are also
wired up as examples


The Blazor WebAssembly.Authentication package supports OIDC logins, AWS Cognito also supports OIDC logins, 
both implementations are decent but both make some assumptions. The Blazor implementation assumes that the
OIDC Identity provider can be managed in a hidden IFrame on the documents page. Cognito blocks click-jacking, 
so it also blocks embedding itself in an IFrame. This causes a 10 to 20 second delay while the JavaScript
authentication library sorts out what is happening. Under the covers, Microsoft uses the open source oidc-js 
JavaScript library, so the Interop/Authentication.js file is actually a slim wrapper of the oidc-js library.
Microsoft also notes that updating the Authentication library is sometimes necesary, they give basic instruction
for how to do that to the MSAL version of the library. The instructions are embelished below with all the information
needed to accomplish this. 
All of this information is to justify the fact that I have indeed modified the WebAssembly.Authentication/ 
Interop/Authentication.js file and commented out all of the IFrame operations. The source was extracted from the
AspNET CORE git hub repository and included here. The source is in the project directory named WebAssembly.Authentication.Interop.
The original Authentication library is named AuthenticationService.Original . To build this project you need to 
install very specific versions of webpack, webpack-cli and other modules. Assuming you have Visual Studio 
and Node.js installed and Yarn and NPM modules installed, follow these steps  
	1.	open a PowerShell command prompt at the root of the WebAssembly.Authentication.Interop folder
	2.	npm install webpack@4.41.5  
	3.	npm install webpack-cli@3.3.10  
	4.	npm install ts-loader@6.2.1  
	5.	npm install typescript@3.7.5  
	6.	npm install oidc-client@1.10.1
To build the project type either 
	yarn build:debug
	-or-
	yarn build
This will compile the Authentication.ts file and put the compiled file in a subdirectory of the dist directory,
you can then copy that into the project. After installing the WebAssembly.Authentication Nuget package, replace
the link in index.html to your updated authentication.js file. 

I compared the source for this file in .NET Core 3.1.1 and 5.0.3, the source for the interop tooling is
identical, so this technique works in both versions of the module. 

The Settings class is a little complicated, mostly as an expiriment but it works fine.
	The appsettings.json file for the client and the server should be the same,
	at least with respect to the security settings, the example below includes
	the settings for the BlazorTest1 cognito group in the Balsamic House Account
	the names are mostly self explanatory. 
{
  "appSettings": {
    "CognitoPoolUrl": "https://cognito-idp.us-east-1.amazonaws.com/us-east-1_mSGIMVY8H",
    "CognitoClientId": "5dbqkegu77qcsqfgpl7l3q0ptp",
    "CognitoNameClaim": "username",
    "CognitoCrmGroupName": "CrmEditor",
    "CognitoScopes": "email,openid,profile",
    "AltAuthorizedApiUrl": "https://localhost:5001/",
    "AuthorizedApiUrl": "https://rw4noi9mtb.execute-api.us-east-1.amazonaws.com/Prod/{proxy+}",
    "AuthorizedClientUrls": "https://localhost:5005,https://localhost:5001",
    "InactivityTimeoutInMinutes": 5
  }
}